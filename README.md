# Teste WebCrawler

Este projeto é teste da eLaw.

## Tecnologias Utilizadas

## Requisitos

- .NET 8 SDK ou superior
- Banco de dados SQL Server - local
- Visual Studio ou Visual Studio Code

## Configuração do Projeto

1. Clone o repositório:
    ```bash
    git clone https://github.com/seu-usuario/task-management-api.git
    ```
    
2. Navegue até o diretório do projeto:
    ```bash
    cd task-management-api
    ```

3. Configure a string de conexão no arquivo `proxies.json`:
    ```json
{    
    "IPAddress": "",
    "Port": "",
    "Country": "",
    "Protocol": ""
  }
    ```

4. Execute as migrações do Entity Framework para criar o banco de dados:
    ```bash
    dotnet ef database update
    ```

5. Compile e execute o projeto:
    ```bash
    dotnet run
    ```

## Documentação e Testes


## Contribuição

Contribuições são bem-vindas! Sinta-se à vontade para abrir issues ou pull requests.

## Licença

Este projeto está licenciado sob os termos da [MIT License](LICENSE).

