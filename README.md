# dapper-query-builder
Build postgres query for Dapper

# How to use
```
public class PostgresQueryBuilder<TEntity> : QueryBuilder<TEntity> where TEntity : class
    {
        public PostgresQueryBuilder() : base(new DbConfig(DbConfig.DbType.Postgres))
        {
            EnableSnakeCase = true;
        }
    }
```
# Insert
```
var queryBuilder = new PostgresQueryBuilder<MasterEntity>();
var entity = new MasterEntity();
entity.MasterId = Guid.NewGuid();
entity.CreatedBy = userInfo.UserId;
entity.CreatedOn = CurrentTime;
entity.ModifiedBy = userInfo.UserId;
entity.ModifiedOn = CurrentTime;
entity.Status = true;
queryBuilder._entity = entity;
var result = queryBuilder.Build(BuildType.Insert);
```
# Entity Structure
```
[Table("Master", Schema = "public")]
public class MasterEntity
{
    [Key]
    public Guid MasterId { get; set; }
    public string Name { get; set; }
    public bool Status { get; set; }
    public long CreatedOn { get; set; }
    public Guid CreatedBy { get; set; }
    public long ModifiedOn { get; set; }
    public Guid ModifiedBy { get; set; }
}
```
