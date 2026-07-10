<img src="Documentation~/banner.png" width="900" alt="Pooling">

[![release](https://img.shields.io/github/v/release/SST-Systems/Pooling)](../../releases)
[![release date](https://img.shields.io/github/release-date/SST-Systems/Pooling)](../../releases)
[![last commit](https://img.shields.io/github/last-commit/SST-Systems/Pooling)](../../commits)
[![license](https://img.shields.io/github/license/SST-Systems/Pooling)](LICENSE.md)

**English** | [Русский](README.ru.md)

---

Reuse objects without allocations.

A lightweight generic object pool for Unity and pure C# projects. Provides `Pool<T>` for a single type and `MultiPool<TKey, TValue>` for managing multiple pools under a common key.

## Table Of Contents

<details>
<summary>Details</summary>

- [Installation](#installation)
- [Classes](#classes)
- [Usage](#usage)
  - [Pool\<T\>](#poolt)
  - [MultiPool\<TKey, TValue\>](#multipooltkey-tvalue)
- [Notes](#notes)

</details>

---

## Installation

1. **.unitypackage** — [Releases](../../releases)
2. **UPM** — `Window → Package Manager` → `+` → `Add package from git URL`:
   `https://github.com/SST-Systems/Pooling.git`
   Append `#tag` to pin a version.
3. **Manual** — clone or download, copy to `Assets/`.

Unity 2021.3+

---

## Classes

**`Pool<T>`** — a single typed pool.

**`MultiPool<TKey, TValue>`** — a dictionary of pools keyed by any type (typically `System.Type`). Lets you manage many pools through a single entry point.

---

## Usage

### Pool\<T\>

```csharp
var pool = new Pool<MyObject>(
    factory: () => new MyObject(),
    actionOnGet: obj => obj.Reset(),
    actionOnRelease: obj => obj.Cleanup()
);

// Prewarm — create instances ahead of time
pool.Prewarm(10);

// Get an instance (creates a new one if the pool is empty)
var obj = pool.Get();

// Return it
pool.Release(obj);

// Permanently remove an instance without returning it
pool.Discard(obj);

// Clear all instances
pool.DiscardAll();
```

### MultiPool\<TKey, TValue\>

```csharp
var multiPool = new MultiPool<Type, IHandler>();

// Register a factory for a specific key
multiPool.RegisterFactory(typeof(MyHandler), () => new MyHandler());

// Get and release by key
var handler = multiPool.Get(typeof(MyHandler));
multiPool.Release(typeof(MyHandler), handler);
```

---

## Notes

- `Pool<T>` tracks both free and occupied instances — double-releasing an instance is silently ignored.
- `MultiPool` throws `KeyNotFoundException` if you call `Get` before registering a factory for that key.

---

## License

Distributed under the [MIT License](LICENSE.md). Free for personal and commercial use.

Author — **Egor Shesterikov**.
