# IAPs модуль

Данный модуль предоставляет возможность легко интегрировать в проект внутриигровые покупки и магазин. 

Поддерживаемые платформы: Android, iOS. Может работать в редакторе. 

Поддерживаемые магазины: AppStore, GooglePlay. При необходимости можно добавлять реализацию покупок в кастомных магазинах. 

# Оглавление


  * [Зависимости и требования](#зависимости-и-требования)
  * [Быстрый старт](#быстрый-старт)
    + [Импорт пакета](#импорт-пакета)
    + [Инстанцирование необходимых сервисов](#инстанцирование-необходимых-сервисов)
  * [Использование](#использование)
  * [Основные классы](#основные-классы)
    + [PurchaseService](#purchaseservice)
    + [IAPService](#iapservice)
    + [OfferConfig](#offerconfig)
    + [ProductConfig](#productconfig)
    + [PurchaseCommand](#purchasecommand)
    + [PurchaseCommandFactory](#purchasecommandfactory)
  * [Вспомогательные классы](#вспомогательные-классы)
    + [StoreConfig](#storeconfig)
    + [OfferGroupConfig](#offergroupconfig)
    + [IOfferLayout](#iofferlayout)
  * [Продвинутое использование](#продвинутое-использование)
    + [Кастомные магазины](#кастомные-магазины)
    + [Пайплайн обработки покупок](#пайплайн-обработки-покупок)
  * [Устаревшее](#устаревшее)
    + [IPurchaseLayout](#ipurchaselayout)

## Зависимости и требования

Для корректной работы модуля IAPs необходимо, чтобы пакет <b> com.littlebitgames.coremodule </b> был установлен. 

Минимальная версия Unity <b> 2021.1.6f1 </b>

## Быстрый старт

### Импорт пакета

Добавьте следующую зависимость в packages.json

```json
 "dependencies": {
    "com.littlebitgames.iapmodule": "https://github.com/LittleBitOrganization/evolution-engine-aip.git"
}
```

### Инстанцирование необходимых сервисов

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
    }

```
## Использование

Все покупки должны совершаться непосредственно через [PurchaseService](#purchaseservice) посредством вызова метода Purchase().

## Основные классы

### PurchaseService

PurchaseService - фасад всего модуля, класс, с которым вам необходимо работать. На данный момент реализован лишь один метод покупки. 

```c#
void Purchase(OfferConfig offerConfig, Action<bool> callback)
```

Метод Purchase() в PurchaseService делегирует покупку сервису IAPService. 

### IAPService

Сервис, являющийся надстройкой над UnityIAP.

Реализует интерфейс IIAPService, его структура следующая:

```c#

        event Action OnPurchasingSuccess;

        event Action OnPurchasingFailed;

        void Purchase(string id);

        void RestorePurchasedProducts(Action<bool> callback);

        ProductWrapper CreateProductWrapper(string id);

```


### OfferConfig

<b> Оффер </b> - это предложение в игровом магазине; то, что игрок покупает. Офферы реализованы в модуле как модель данных (ScriptableObject), которая описывает идентификатор покупки, тип покупки, а также набор получаемых [продуктов](#продукты).

Можно создать OfferConfig в меню "Configs/Store Configs/Offer Config".

OfferConfig также может иметь ссылку на [IOfferLayout](#iofferlayout).

Структура:

```swift
id:string -> id оффера (покупки)

productType:ProductType -> тип покупки (Consumable, NonConsumable, Subscription)

layout:IPurchaseInterfaceContainer -> ссылка на UI view

products:IReadOnlyList<ProductConfig> -> список покупаемых продуктов
```

Важно! id должен быть написан в lower case стиле, единственный допустимый сепаратор - <b> нижнее подчеркивание </b>. Для проверки id на соответствие нажмите кнопку <b> Validate </b>

### ProductConfig

<b> Продукт </b> - это любой открываемый контент. Часто выделяются следующие типы продуктов: "NoAds", внутриигровые ресурсы, скины и т.д. Продукты являются состовляющей частью [офферов](#offerconfig).  К примеру, оффер "Стартер Пак" может содержать в себе такие продукты как "100 золота" и "No Ads". Оффер "500 алмазов" может содержать в себе лишь один продукт - "500 алмазов".

Вы можете создать конфиг продукта любого типа. Для этого необходимо унаследоваться от ProductConfig и переопределить метод HandlePurchase().

Структура конфига вашего продукта может быть любой, неизменным остается метод HandlePurchase()

HandlePurchase() вызывается [PurchaseService](#purchaseservice) в момент покупки оффера, содержащего данный продукт. 

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


NoAdsProductConfig переопределяет метод HandlePurchase, вызывая необходимую [команду покупки No Ads](#purchasecommand). Команды создаются с помощью [PurchaseCommandFactory](#purchasecommandfactory)

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

## Вспомогательные классы

Вспомогательные конфиги не обязательны к использованию.

### StoreConfig

Конфиг, который содержит в себе группы офферов.

Содержит единственное поле - List<[OffersGroupConfig](#offergroupconfig)> groups


### OfferGroupConfig

Конфиг, помогающий объединить по смыслу группу офферов. Например, группа ресусров, группа паков с NPC. [StoreConfig](#storeconfig) содержит в себе группы офферов. 

Структура:

```swift
titie:string -> название группы

description:string -> описание группы

icon:sprite -> иконка группы

offers:IReadOnlyList<OfferConfig> -> офферы, находящиеся в данной группе

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

### Пайплайн обработки покупок

Пайплайн обработки покупок следующий:

[PurchaseService](#purchaseservice).Purchase() -> [IAPService](#iapservice).Purchase() -> UnityIAP -> ...

При необходимости можно сделать свою надстройку над IAPService (вместо готового PurchaseService). 

## Устаревшее

### IPurchaseLayout

Интерфейс устарел. Вместо IPurchaseLayout следует использовать [IOfferLayout](#iofferlayout). 


