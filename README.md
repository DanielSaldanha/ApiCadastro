# API Cadastro

Uma API RESTful desenvolvida em ASP.NET Core 8.0 para gerenciamento de cadastro de usuários com autenticação JWT, sistema de créditos e armazenamento em cache.

## 📋 Funcionalidades

- ✅ **Autenticação JWT** - Geração e validação de tokens seguros
- ✅ **Cadastro de Usuários** - Registro com validações rigorosas
- ✅ **Sistema de Créditos** - Limite de requisições através de Redis
- ✅ **Caching** - Redis para performance e cache em memória
- ✅ **Banco de Dados** - MySQL com Entity Framework Core
- ✅ **Documentação Swagger** - Interface interativa para testar endpoints
- ✅ **Testes Unitários** - Cobertura de testes para controllers e models
- ✅ **Validação de Dados** - Regras de validação em DTOs

## 🛠️ Tecnologias Utilizadas

- **Framework**: ASP.NET Core 8.0
- **Banco de Dados**: MySQL 8.0
- **ORM**: Entity Framework Core
- **Autenticação**: JWT (JSON Web Tokens)
- **Cache**: Redis (StackExchange.Redis)
- **Criptografia**: BCrypt.Net
- **Documentação**: Swagger/OpenAPI
- **Testes**: NUnit/xUnit

## 📁 Estrutura do Projeto

```
ApiCadastro/
├── Controllers/              # Controladores da API
│   ├── Auth.cs             # Endpoints de autenticação
│   └── UserController.cs    # Endpoints de gerenciamento de usuários
├── Model/                   # Modelos de dados
│   ├── User.cs            # Entidade de usuário
│   └── DTO.cs             # Objetos de transferência de dados
├── Data/                    # Contexto do banco de dados
│   └── AppDbContext.cs     # DbContext do Entity Framework
├── Credit/                  # Serviço de sistema de créditos
│   └── CreditService.cs    # Lógica de limite de requisições
├── Middlewares/             # Middlewares customizados
│   └── JwtMiddleware.cs    # Middleware para validação JWT
├── TokenPaste/              # Gerenciamento de tokens
│   ├── CreateToken.cs      # Geração de tokens
│   └── ValidatorToken.cs   # Validação de tokens
├── Testing/                 # Testes da aplicação
│   ├── ControllerToTestings.cs
│   ├── Controller/
│   │   ├── AuthenticationTest.cs
│   │   ├── TestControllerToTestings.cs
│   │   └── TestesIntegrados.cs
│   └── ModelTest/
│       └── UserModel.cs
└── Properties/              # Configurações do projeto
    └── launchSettings.json
```

## 🔧 Requisitos

- .NET 8.0 SDK ou superior
- MySQL Server 8.0 ou superior
- Redis Server
- Visual Studio 2022 ou VS Code

## 🚀 Instalação e Configuração

### 1. Clonar o Repositório
```bash
git clone <seu-repositorio>
cd ApiCadastro
```

### 2. Configurar Banco de Dados

Edite o arquivo `appsettings.json` com suas credenciais MySQL:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=MyDb;User=root;Password=SUA_SENHA"
  }
}
```

### 3. Aplicar Migrations

```bash
dotnet ef database update
```

### 4. Configurar Redis

Certifique-se de que Redis está rodando localmente ou configure a URL no `appsettings.json`.

### 5. Executar a Aplicação

```bash
dotnet run
```

A API estará disponível em: `https://localhost:5001`

## 📚 Endpoints Principais

### Autenticação

#### Gerar Token
```http
POST /gerartoken
Content-Type: application/json

{
  "username": "seu_usuario",
  "password": "SuaSenha123!"
}
```

**Resposta (200 OK):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

#### Logout
```http
POST /logout
Content-Type: application/json

"seu_token_jwt"
```

### Usuários

#### Registrar Novo Usuário
```http
POST /registrar
Authorization: Bearer {seu_token}
Content-Type: application/json

{
  "nome": "João Silva",
  "email": "joao@example.com",
  "password": "SuaSenha123!",
  "profissao": "Desenvolvedor",
  "cargo": "Senior",
  "nascimento": "1990-05-15"
}
```

## ✅ Validações

### Modelo User/DTO

- **Nome**: Obrigatório
- **Email**: Obrigatório, deve ser um email válido
- **Senha**: 
  - Mínimo 8 caracteres
  - Deve conter: letra maiúscula, minúscula, número e caractere especial (@$!%*?&)
- **Data de Nascimento**: 
  - Obrigatória
  - Usuário deve ter entre 18 e 65 anos (na maioria dos endpoints)
- **Profissão**: Opcional
- **Cargo**: Opcional

### Exemplo de Validação de Senha
✅ Válido: `Password123!@#`  
❌ Inválido: `password` (sem maiúscula, número ou caractere especial)

## 🔐 Segurança

- **JWT Bearer Token** para autenticação
- **Middleware JWT** para validação em endpoints protegidos
- **BCrypt** para hash de senhas
- **Revogação de Tokens** via logout
- **Validação de entrada** em todos os DTOs
- **CORS** configurável

## 💾 Caching

A aplicação utiliza dois níveis de cache:

1. **Redis** (Cache Distribuído)
   - Armazenamento de sessão
   - Sistema de créditos/limite de requisições
   - Dados compartilhados entre instâncias

2. **Memory Cache** (Cache em Memória)
   - Cache local da aplicação
   - Performance para dados de acesso frequente

## 📊 Sistema de Créditos

- **Limite**: 10 requisições por período
- **Verificação**: `CreditService.Verificador()`
- **Armazenamento**: Redis
- **Middleware**: Validação automática em endpoints protegidos

## 🧪 Testes

Execute os testes com:

```bash
dotnet test
```

Cobertura de testes:
- ✅ Autenticação (geração e validação de tokens)
- ✅ Validação de usuários
- ✅ Controllers (Auth, UserController)
- ✅ Modelos de dados

## 📖 Documentação Swagger

Acesse a documentação interativa em:

```
https://localhost:5001/swagger/index.html
```

No Swagger, você pode:
- Visualizar todos os endpoints
- Testar requisições direto da interface
- Ver os modelos de requisição/resposta
- Autenticar com token JWT

## 🐛 Troubleshooting

### Erro de Conexão MySQL
```
Verify ConnectionString in appsettings.json
Check if MySQL Server is running
```

### Erro de Redis
```
Ensure Redis is running on default port (6379)
Check ConnectionString in configuration
```

### Erro de Autenticação
```
Token may be expired or invalid
Ensure JWT middleware is configured in Program.cs
```

## 📝 Configuração do Program.cs

A aplicação está configurada com:
- Swagger/OpenAPI
- DbContext para MySQL
- Redis Cache (Distributed)
- Autenticação JWT com Bearer
- CORS habilitado

## 🔄 Fluxo de Autenticação

1. Usuário faz login com credenciais
2. Sistema valida credenciais no banco
3. Token JWT é gerado (contém informações do usuário)
4. Cliente inclui token em `Authorization: Bearer {token}`
5. Middleware valida token em cada requisição
6. Logout revoga o token adicionando-o à lista de tokens revogados

## 📧 Contato e Contribuições

Para dúvidas ou sugestões, abra uma issue ou entre em contato.

## 📄 Licença

Este projeto é fornecido como está, sem garantias.

---

**Última atualização**: Abril de 2026
