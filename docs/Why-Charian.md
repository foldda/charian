# Why Charian

Protobuf, Avro, and JSON Schema all solve data exchange by fixing a schema up front and generating code from it. That works well when both sides of a connection are owned by the same team and evolve together. Charian's **self-binding** skips the schema entirely, so there's nothing to keep in sync in the first place — instead, each object resolves its own state at read-time, reading only the fields it needs and tolerating whatever else is or isn't there, and the connecting systems are responsible for managing the established, or evolved, data models.

The result is **tight coupling vs. loose coupling**: Protobuf and Avro require both sides of a connection to share a schema — that shared contract is what enables their compactness and compile-time validation, but it also means both sides must stay synchronized as the data model changes; Charian removes the shared schema, trading those benefits for structural independence between systems that don't evolve together.

| | Protobuf / Avro / JSON Schema | Charian |
|---|---|---|
| **Contract** | Schema defined up front; both sides must agree on it | No schema; each side reads/writes by position, independently |
| **Schema drift** | A field added, removed, or retyped on one side can break the other unless versioning rules are followed carefully | Structure changes on one side don't break the other — the receiver only reads what it expects and handles the rest itself |
| **Setup** | Requires a schema file (`.proto`, `.avsc`, etc.) and a code-generation step | No schema file, no codegen — just two source files added to your project |
| **Tooling footprint** | Compiler/codegen toolchain, schema registry (for Avro), versioning discipline | None — ~800 lines, no 3rd-party dependency |
| **Validation** | Strong compile-time type safety and schema enforcement, made possible by the shared contract | None — validation and error handling are the client's responsibility |
| **Payload size** | Compact binary for Protobuf/Avro; the shared schema lets field names and type tags be stripped from the wire | Delimited text string, larger than Protobuf/Avro's binary but more compact than JSON |
| **Best fit** | Stable, high-throughput systems within a single team's control (internal microservices, high-volume event streams) | Systems integration across teams, vendors, or legacy platforms where data models are inconsistent, evolving, or outside your control |

Charian is aimed at the specific pain point of **integration between systems you don't fully control** — connecting a legacy system to a modern one, exchanging data with a third-party vendor, or maintaining a pipeline where the data model on either end changes independently and without warning. In these situations, a shared schema becomes a liability: every change on one side risks a synchronized (and often coordinated, multi-team) update on the other, or the pipeline breaks.

Charian unique approach of self-binding allows flexible handling of data's schema changes, even processing data of multiple schema versions concurrently and dynamically. This trades the schema's built-in validation for structural independence — a fair trade when the alternative is fragile, tightly-coupled pipelines between systems that were never designed to evolve together.
