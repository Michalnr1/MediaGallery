using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// --- KONFIGURACJA AWS DYNAMODB ---
var dynamoDbUrl = builder.Configuration.GetValue<string>("DynamoDB:ServiceURL");
var dynamoDbConfig = new AmazonDynamoDBConfig();
if (!string.IsNullOrEmpty(dynamoDbUrl))
{
    dynamoDbConfig.ServiceURL = dynamoDbUrl;
}

// Dla lokalnego DynamoDB wymagane są jakiekolwiek (nawet fałszywe) poświadczenia
var credentials = new Amazon.Runtime.BasicAWSCredentials("fakeaccesskey", "fakesecretkey");
var dynamoDbClient = new AmazonDynamoDBClient(credentials, dynamoDbConfig);

builder.Services.AddSingleton<IAmazonDynamoDB>(dynamoDbClient);
builder.Services.AddScoped<IDynamoDBContext, DynamoDBContext>();

var app = builder.Build();

// --- AUTOMATYCZNE TWORZENIE TABELI (tylko do testów lokalnych) ---
await EnsureTableCreatedAsync(dynamoDbClient);

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("AllowFrontend");
//app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Funkcja pomocnicza tworząca tabelę 'MediaItems' jeśli nie istnieje
async Task EnsureTableCreatedAsync(IAmazonDynamoDB client)
{
    var tableName = "MediaItems";
    var tables = await client.ListTablesAsync();

    if (!tables.TableNames.Contains(tableName))
    {
        var request = new CreateTableRequest
        {
            TableName = tableName,
            AttributeDefinitions = new List<AttributeDefinition>
            {
                new AttributeDefinition("UserId", ScalarAttributeType.S),
                new AttributeDefinition("MediaId", ScalarAttributeType.S)
            },
            KeySchema = new List<KeySchemaElement>
            {
                new KeySchemaElement("UserId", KeyType.HASH),  // Klucz główny
                new KeySchemaElement("MediaId", KeyType.RANGE) // Klucz sortujący
            },
            BillingMode = BillingMode.PAY_PER_REQUEST
        };
        await client.CreateTableAsync(request);
        Console.WriteLine($"[INIT] Tabela {tableName} została utworzona.");
    }
}