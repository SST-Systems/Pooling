<img src="Documentation~/banner.png" width="900" alt="Pooling">

[![release](https://img.shields.io/github/v/release/SST-Systems/Pooling)](../../releases)
[![release date](https://img.shields.io/github/release-date/SST-Systems/Pooling)](../../releases)
[![last commit](https://img.shields.io/github/last-commit/SST-Systems/Pooling)](../../commits)
[![license](https://img.shields.io/github/license/SST-Systems/Pooling)](LICENSE.md)

[English](README.md) | **Русский**

---

Переиспользуй объекты без аллокаций.

Лёгкий generic-пул объектов для Unity и чистых C#-проектов. Включает `Pool<T>` для одного типа и `MultiPool<TKey, TValue>` для управления несколькими пулами под общим ключом.

## Содержание

<details>
<summary>Развернуть</summary>

- [Установка](#установка)
- [Классы](#классы)
- [Использование](#использование)
  - [Pool\<T\>](#poolt)
  - [MultiPool\<TKey, TValue\>](#multipooltkey-tvalue)
- [Примечания](#примечания)

</details>

---

## Установка

1. **.unitypackage** — [Releases](../../releases)
2. **UPM** — `Window → Package Manager` → `+` → `Add package from git URL`:
   `https://github.com/SST-Systems/Pooling.git`
   Добавь `#тег` в конец URL для фиксации версии.
3. **Вручную** — склонируй или скачай, скопируй в `Assets/`.

Unity 2021.3+

---

## Классы

**`Pool<T>`** — одиночный типизированный пул.

**`MultiPool<TKey, TValue>`** — словарь пулов с произвольным ключом (как правило `System.Type`). Позволяет управлять множеством пулов через одну точку входа.

---

## Использование

### Pool\<T\>

```csharp
var pool = new Pool<MyObject>(
    factory: () => new MyObject(),
    actionOnGet: obj => obj.Reset(),
    actionOnRelease: obj => obj.Cleanup()
);

// Прогрев — создать экземпляры заранее
pool.Prewarm(10);

// Получить экземпляр (создаёт новый, если пул пуст)
var obj = pool.Get();

// Вернуть обратно
pool.Release(obj);

// Удалить экземпляр без возврата в пул
pool.Discard(obj);

// Очистить все экземпляры
pool.DiscardAll();
```

### MultiPool\<TKey, TValue\>

```csharp
var multiPool = new MultiPool<Type, IHandler>();

// Зарегистрировать фабрику для ключа
multiPool.RegisterFactory(typeof(MyHandler), () => new MyHandler());

// Взять и вернуть по ключу
var handler = multiPool.Get(typeof(MyHandler));
multiPool.Release(typeof(MyHandler), handler);
```

---

## Примечания

- `Pool<T>` отслеживает и свободные, и занятые экземпляры — двойной `Release` игнорируется без ошибок.
- `MultiPool` выбрасывает `KeyNotFoundException`, если вызвать `Get` до регистрации фабрики для этого ключа.

---

## Лицензия

Распространяется под [MIT License](LICENSE.md). Свободно для использования в личных и коммерческих проектах.

Автор — **Egor Shesterikov**.
