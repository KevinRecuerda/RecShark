# Data.Db.Document

Store document database, using `Postgresql` and `Marten`

> see more [here](https://github.com/KevinRecuerda/DocumentStore)



### Quick start

- Implement `IConnectionString`
- Implement `DocumentStoreFactory` inheriting from `BaseDocumentStoreFactory`
- DI: add module `DocumentStoreDataModule<TFactory, TConnection>`



### Features

##### Query
- `BaseDocumentDataAccess`

##### Initialization
- Views `FeatureSchemaViews`
- Data `DataChange` &rarr; works as liquibase

##### Extensions
- Projection `SelectFields`
- Condition `Where extended with include` / `Between` / `ContainsAny` / `In`
- Condition `Latest`
- Search `SearchSimilar`
- Index `Gin`/`Gist`
