using IdentityModel.Client;

Console.WriteLine("Hello, World!");

var client = new HttpClient();

var configuration = await client.GetDiscoveryDocumentAsync("https://localhost:7275/");
if (configuration.IsError)
    throw new Exception($"An error occurred while retrieving the configuration document: {configuration.Error}");

var response = await client.RequestPasswordTokenAsync(new PasswordTokenRequest()
{
    Address = configuration.TokenEndpoint,
    UserName = "admin@outlook.com",
    Password = "Password12*"
});

if (response.IsError) throw new Exception($"An error occurred while retrieving an access token: {response.Error}");

Console.WriteLine($"Token : {response.AccessToken}");

client.SetBearerToken(response.AccessToken);

var microserviceResponse = await client.PostAsync("https://localhost:7083/api/example", null);

var microserviceResponseContent = await microserviceResponse.Content.ReadAsStringAsync();
Console.WriteLine($"Microservice response :  {microserviceResponseContent}");