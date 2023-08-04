# IAPs модуль

Данный модуль предоставляет возможность легко интегрировать в проект внутриигровые покупки и магазин. 

Поддерживаемые платформы: Android, iOS. Может работать в редакторе. 

Поддерживаемые магазины: AppStore, GooglePlay. При необходимости можно добавлять реализацию покупок в кастомных магазинах. 

# Оглавление

- [Зависимости и требования](#зависимости-и-требования)
  * [Импорт](#импорт)
- [Инстанцирование необходимых сервисов](#инстанцирование-необходимых-сервисов)
- [Быстрый старт](#быстрый-старт)
- [Основные классы](#основные-классы)
  * [PurchaseService](#purchaseservice)
  * [IAPService](#iapservice)
  * [OfferConfig](#offerconfig)
  * [ProductConfig](#productconfig)
  * [PurchaseCommand](#purchasecommand)
  * [PurchaseCommandFactory](#purchasecommandfactory)
  * [IProductWrapper](#iproductwrapper)
- [Тестирование](#тестирование)
- [Вспомогательные классы](#вспомогательные-классы)
  * [StoreConfig](#storeconfig)
  * [OfferGroupConfig](#offergroupconfig)
  * [IOfferLayout](#iofferlayout)
- [Продвинутое использование](#продвинутое-использование)
  * [Кастомные магазины](#кастомные-магазины)
- [Устаревшее](#устаревшее)
  * [IPurchaseLayout](#ipurchaselayout)

## Зависимости и требования

Для корректной работы модуля IAPs необходимо, чтобы пакет <b> com.littlebitgames.coremodule </b> был установлен. 

Минимальная версия Unity <b> 2021.1.6f1 </b>

### Импорт

Добавьте следующую зависимость в packages.json

```json
 "dependencies": {
    "com.dbrizov.naughtyattributes": "https://github.com/dbrizov/NaughtyAttributes.git#upm",
    "com.littlebitgames.iapmodule": "https://github.com/LittleBitOrganization/iap-billing-include.git"
}
```

## Инстанцирование необходимых сервисов

</b> Пример инсталлера, если используется Zenject</b>.

Инстанцирование сервисов без Zenject происходит аналогично. Главное не забыть создать необходимую реализацию ICreator (см. core-модуль).


```c#
public override void InstallBindings()
        {
            Container
                .Bind<PurchaseService>()
                .FromSubContainerResolve()
                .ByMethod(InstallPurchaseService)
                .AsSingle();
        }

        private void InstallPurchaseService(DiContainer container)
        {
            container
                .Bind<ICreator>()
                .To<CreatorInDiContainer>()
                .AsSingle()
                .Lazy();

            container
                .Bind<PurchaseCommandFactory>()
                .AsSingle()
                .NonLazy();

            container
                .Bind<PurchaseService>()
                .AsSingle()
                .NonLazy();

            container
                .Bind<IIAPService>()
                .To<IAPService>()
                .AsSingle()
                .NonLazy();

            container
                .Bind<IPurchaseHandler>()
                .FromInstance(GetPurchaseHandler())
                .AsSingle()
                .NonLazy();

            container
                .Bind<ITransactionsRestorer>()
                .FromInstance(GetTransactionsRestorer())
                .AsSingle()
                .NonLazy();
        }

        private static IPurchaseHandler GetPurchaseHandler() => Application.platform switch
        {
            RuntimePlatform.Android or RuntimePlatform.IPhonePlayer => new CrossPlatformPurchaseHandler(),
            _ => new DebugPurchaseHandler()
        };

        private static ITransactionsRestorer GetTransactionsRestorer() => Application.platform switch
        {
            RuntimePlatform.Android => new GooglePlayTransactionsRestorer(),
            RuntimePlatform.IPhonePlayer => new AppleTransactionsRestorer(),
            _ => new DebugTransactionsRestorer()
        };
   

```

## Быстрый старт

Для совершения покупки используйте метод Purchase() в [PurchaseService](#purchaseservice) и передайте созданный [Offer Config](#offerconfig):

```c#
purchaseService.Purchase(offerConfig, success => Debug.Log(string.Format("Purchase was processed with status:{0}",success));
```

Для доступа к данным о продутке создайте IProductWrapper с помощью [PurchaseService](#purchaseservice):

```c#
IProductWrapper product = purchaseService.GetProductWrapper(offerConfig);

Debug.Log(product.LocalizedPrice);                   // output -> 1
Debug.Log(product.LocalizedPriceString);             // output -> 1$
Debug.Log(product.LocalizedTitle);                   // output -> "50 diamonds"
Debug.Log(product.LocalizedTitle);                   // output -> "a money bag"
Debug.Log(product.CanPurchase);                      // output -> true
Debug.Log(product.IsPurchased);                      // output -> false
```

Нужно иметь в виду, что метод GetProductWrapper вернет null, если в момент обращения к нему [IAPService](#iapservice) не будет инициализирован. 
[PurchaseService](#purchaseservice) имеет событие OnInitialized и свойство IsInitialized. IsInitialized вернет true, если [IAPService](#iapservice) был инициализирован:

```c#
purchaseService.OnInitialized += () => {
    IProductWrapper product = purchaseService.GetProductWrapper(offerConfig);    
    Debug.Log(product.LocalizedPrice);
 };

```

Смотрите также главу [тестирование](#тестирование).

## Основные классы

### PurchaseService

PurchaseService - фасад всего модуля, класс, с которым вам необходимо работать. Необходим для удобства использования функционала покупок, работать с IAPService напрямую не очень удобно. Структура следующая:

```c#

public event Action OnInitialized;

public bool IsInitialized { get; private set; }

public IProductWrapper GetProductWrapper(string id);

public IProductWrapper GetProductWrapper(OfferConfig offerConfig);

void Purchase(OfferConfig offerConfig, Action<bool> callback);
```

Методы Purchase() и GetProductWrapper() делигируют логику выполнения [IAPService](#iapservice). Поэтому при желании вы можете написать свой PurchaseService -> обертку над [IAPService](#iapservice).

### IAPService

Сервис, являющийся надстройкой над UnityIAP.

Реализует интерфейс IIAPService, его структура следующая:

```c#

event Action OnPurchasingSuccess;

event Action OnPurchasingFailed;

void Purchase(string id);

void RestorePurchasedProducts(Action<bool> callback);

IProductWrapper GetProductWrapper(string id);

```
Вы можете работать с ним напрямую, однако лучше написать свою обертку или использовать готовую - [PurchaseService](#purchaseservice).

### OfferConfig

<b> Оффер </b> - это предложение в игровом магазине; то, что игрок покупает. Офферы реализованы в модуле как модель (ScriptableObject), которая описывает идентификатор покупки, тип покупки, а также набор получаемых [продуктов](#продукты).

Можно создать OfferConfig в меню "Configs/Store Configs/Offer Config".

OfferConfig также может иметь ссылку на [IOfferLayout](#iofferlayout).

Структура:

```c#
string id; -> id оффера (покупки)

ProductType productType; -> тип покупки (Consumable, NonConsumable, Subscription)

IPurchaseInterfaceContainer layout; -> ссылка на UI view

IReadOnlyList<ProductConfig> products; -> список покупаемых продуктов
```

Важно! id должен быть написан в lower case стиле, единственный допустимый сепаратор - <b> нижнее подчеркивание </b>. Для проверки id на соответствие нажмите кнопку <b> Validate </b>. ID должен соответствовать ID соответствующего продукта в магазине. 

На основе OfferConfig создается [IProductWrapper](#iproductwrapper). Если игра запущена в режиме отладки (см. [тестирование](#тестирование)), то ProductType в IProductWrapper будет соответствовать ProductType из OfferConfig. В режиме релиза информация о типе продукта для IProductWrapper подтягивается из магазина. 

### ProductConfig

<b> Продукт </b> - это любой открываемый контент. Часто выделяются следующие типы продуктов: "NoAds", внутриигровые ресурсы, скины и т.д. Продукты являются состовляющей частью [офферов](#offerconfig).  К примеру, оффер "Стартер Пак" может содержать в себе такие продукты как "100 золота" и "No Ads". Оффер "500 алмазов" может содержать в себе лишь один продукт - "500 алмазов".

Вы можете создать конфиг продукта любого типа. Для этого необходимо унаследоваться от ProductConfig и переопределить метод HandlePurchase().

Структура конфига вашего продукта может быть любой, неизменным остается метод HandlePurchase()

HandlePurchase() вызывается в момент покупки оффера, содержащего данный продукт. 

<b> Пример реализации продукта "NoAds": </b>

NoAdsProductConfig.cs

```c#

    [CreateAssetMenu(fileName = "NoAdsProductConfig", menuName = "Configs/Store Configs/NoAds")]
    public class NoAdsProductConfig : ProductConfig
    {
        public override void HandlePurchase(PurchaseCommandFactory purchaseCommandFactory)
        {
            purchaseCommandFactory.Create<PurchaseNoAdsCommand>().Execute();
        }
    }

```

PurchaseNoAdsCommand.cs

```c#
    public class PurchaseNoAdsCommand : IPurchaseCommand
    {
        public void Execute()
        {
            PurchaseNoAds(true);
        }

        public void Undo()
        {
            PurchaseNoAds(false);
        }

        private void PurchaseNoAds(bool purchased)
        {
            PlayerPrefs.SetInt("NoAdsPurchased",purchased ? 1 : 0);
        }
    }

```


NoAdsProductConfig переопределяет метод HandlePurchase, создавая и выполняя необходимую [команду покупки No Ads](#purchasecommand). Команды создаются с помощью [PurchaseCommandFactory](#purchasecommandfactory)

### PurchaseCommand

Команда покупки описывается интерфейсом IPurchaseCommand. 

Для каждого отдельного типа продукта необходимо создать собственную команду покупки.

Нужно реализовать методы Execute() и Undo()

void Execute() -> реализация покупки конкретного продукта (открытие нового контента, начисление вирт. валюты и проч.)

void Undo() -> отмена покупки. Если отмену нельзя обработать безболезненно (например, отмена начисления виртуальной валюты), следует реализовать пустой метод.  

Дополнительные аргументы для выполнения и отмены команды нужно указывать в конструкторе команды. 

### PurchaseCommandFactory

Фабрика, предназначенная для инстанцирования команд покупок. 
Имеет лишь один публичный метод:

```c#
Create<T>(), T реализует интерфейс IPurchaseCommand.
```

Его также можно вызвать с аргументами (не обязательный параметр object[] args). Они будут переданы в конструктор команды покупки.  

### IProductWrapper

IProductWrapper - обертка над UnityEngine.Purchasing.Product. Представляет собой единый интерфейс для работы с данными о продукте, полученными с сервера магазина (например, цена или описание). Может быть создан посредством метода GetProductWrapper(id/offer-config) в [PurchaseService](#purchaseservice). В режиме [тестирования](#тестирование) возвращает следующие тестовые данные:

LocalizedPrice - 1
LocalizedPriceString - 1$
Title - "Sample Product"
Description - "Sample Product Description"
IsPurchased - куплен ли продукт (данные сохраняются в PlayerPrefs)
CanPurchase - можно ли купить продукт (значение возвращается в зависимости от типа продукта, указанного в ProductConfig)

В [продакшене](#тестирование) возвращает данные, полученные с сервера соответствующего магазина (GooglePlay, AppStore)

## Тестирование

В режиме редактирования модуль всегда работает как в тестовом режиме. Для того, чтобы запустить тестовый режим на устройстве, добавьте <b>IAP_DEBUG</b> define в Scripting Define Symbols в Player Settings.

С точки зрения проектирования кода - ваша реализация не должна отличаться ни в режиме отладки (UNITY_EDITOR || IAP_DEBUG), ни в режиме релиза. В режиме отладки модуль не использует непосредственно API UNITY_IAP, а [IProductWrapper](#iproductwrapper) возвращает тестовые данные.

Пайплайн обработки покупок в режиме релиза следующий:

```
[PurchaseService](#purchaseservice).Purchase() -> [IAPService](#iapservice).Purchase() -> UnityIAP -> ... -> UnityIAP -> PurchaseComplete!
```

В режиме тестирования:

```
[PurchaseService](#purchaseservice).Purchase() -> [IAPService](#iapservice).Purchase() -> FakePurchase -> PurchaseComplete!
```

## Вспомогательные классы

Вспомогательные конфиги не обязательны к использованию.

### StoreConfig

Конфиг, который содержит в себе группы офферов.

Содержит единственное поле - List<[OffersGroupConfig](#offergroupconfig)> groups

### OfferGroupConfig

Конфиг, помогающий объединить по смыслу группу офферов. Например, группа ресусров, группа паков с NPC. [StoreConfig](#storeconfig) содержит в себе группы офферов. 

Структура:

```c#
string titie -> название группы

string description -> описание группы

Sprite icon -> иконка группы

IReadOnlyList<OfferConfig> offers -> офферы, находящиеся в данной группе

```

### IOfferLayout

Единый интерфейс логики элементов UI, отвечающих за отображение офферов. 

Структура:

```c#
event Action OnClickBuy -> следует вызывать при нажатии на кнопку покупки

void SetData(OfferConfig) -> установка данных об оффере

void SetButtonInteractable(bool) -> метод для изменения состояния интерактивности кнопки
```

Пример использования. В проекте GardenEvolution отрисовка паков с садовниками в магазине происходит следующим образом:

В момент открытия магазина проходимся for-each лупом по всем OfferConfig, находящимся в [StoreConfig](#storeconfig). Далее для каждого OfferConfig выдергиваем поле layout(IOfferLayout) и создаваем его uGUI префаб с помощью ILayoutFactory (см. ui-модуль). После каждому созданному префабу с помощью метода SetData передаем конфиг оффера. View-классы паков, которые теперь имеет данные о соответствующем OfferConfig, сами отрисовывают садовников [продукты](#productconfig). 

## Продвинутое использование

### Кастомные магазины

При желании имплементировать в игру другие магазины (https://docs.unity3d.com/Manual/UnityIAPImplementingAStore.html), не забудь создать реализацию логики для:
* IPurchaseHandler 
* ITransactionsRestorer

## Устаревшее

### IPurchaseLayout

Интерфейс устарел. Вместо IPurchaseLayout следует использовать [IOfferLayout](#iofferlayout). 


