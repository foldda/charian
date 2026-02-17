# Charian (.NET)

A tiny, dependency‑free serializer that packs any object graph into a **plain text string** (RDA) for storage or cross‑language exchange.

## Install
```powershell
dotnet add package Foldda.Charian
```

## A 30‑second example - 

```C#
using Charian;

// from a sending app ...
var person = new Person { FirstName = "Ada", LastName = "Lovelace" };
var rda = person.ToRda();
string s = rda.ToString();   // store or send the string

// from a receiving app ...
var restored = new Person().FromRda(Charian.Rda.Parse(s));

```

## License & Commercial Use

GPL‑3.0. 

Commercial licenses and support available — contact@foldda.com

