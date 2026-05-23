# MonitorAcessoCatraca

Monitor de acessos em segundo plano para o **Controle de Acesso da Next Fit**, desenvolvido em **C# Windows Forms (.NET Framework)**.

---

## O que lhe motivou a fazer o projeto

O projeto foi motivado por solicitações de clientes sobre a necessidade de visualizar de forma mais rápida e clara as tentativas de acesso realizadas no Controle de Acesso.

A proposta surgiu como uma melhoria de usabilidade para o sistema, permitindo que a recepção ou os responsáveis pelo ambiente recebam uma notificação visual sempre que um acesso for realizado, sem depender da consulta manual de relatórios ou da observação constante da tela principal do Controle de Acesso.

---

## Quais os problemas que seu projeto visa resolver

O projeto busca resolver os seguintes pontos:

- dificuldade de acompanhar acessos em tempo real;
- necessidade de consultar relatórios manualmente para verificar acessos;
- baixa visibilidade sobre acessos bloqueados;
- ausência de uma notificação rápida para a recepção;
- melhoria da experiência de uso para clientes que utilizam controle de acesso com catraca ou leitor facial.

Com o monitor, o usuário recebe uma notificação no canto inferior direito do Windows informando se o acesso foi **liberado** ou **bloqueado**.

---

## Descreva brevemente como funciona seu projeto

O `MonitorAcessoCatraca` funciona em segundo plano na bandeja do Windows.

Ao ser iniciado, ele verifica se o `ControleAcesso.exe` está aberto e acompanha a comunicação realizada pelo Controle de Acesso durante a validação automática de entrada.

Quando uma tentativa de acesso é identificada, o sistema interpreta a resposta recebida e exibe uma notificação com as principais informações do acesso.

A notificação pode conter:

- nome do cliente;
- status do acesso;
- horário;
- serviço ou contrato, quando disponível;
- motivo do bloqueio, quando houver.

---

## Quais as tecnologias utilizadas no projeto

| Tecnologia | Utilização |
|---|---|
| C# | Linguagem principal do projeto |
| Windows Forms | Interface gráfica do sistema |
| .NET Framework 4.7.2 / 4.8 | Plataforma de execução |
| Newtonsoft.Json | Manipulação de JSON |
| System.Data.SQLite.Core | Consulta ao banco local quando necessário |
| Titanium.Web.Proxy | Monitoramento da comunicação HTTPS |
| Git | Controle de versão |
| GitHub | Hospedagem do repositório |

---

## Configuração do ambiente

O projeto utiliza um arquivo `.env` para armazenar configurações do endpoint monitorado.

Crie um arquivo `.env` na raiz do projeto ou junto ao executável.

Exemplo:

```env
HOST_ACESSO=
ENDPOINT_ACESSO_AUTOMATICO=
```

O arquivo `.env` não deve ser enviado ao Git.

Mantenha no repositório apenas um arquivo `.env.example` com as variáveis vazias:

```env
HOST_ACESSO=
ENDPOINT_ACESSO_AUTOMATICO=
```

---

## Pacotes NuGet necessários

Instale os pacotes abaixo no projeto:

```powershell
Install-Package Newtonsoft.Json
Install-Package System.Data.SQLite.Core
Install-Package Titanium.Web.Proxy
```

---

## Estrutura principal do projeto

```text
MonitorAcessoCatraca
├── Config
├── DTOs
├── Forms
├── Models
├── Services
├── Utils
├── App.config
├── packages.config
├── Program.cs
└── MonitorAcessoCatraca.csproj
```

---

## Como executar o projeto

1. Abra a solução no Visual Studio.
2. Restaure os pacotes NuGet.
3. Crie o arquivo `.env`.
4. Compile o projeto.
5. Execute o `MonitorAcessoCatraca`.
6. Permita a instalação do certificado, se solicitado.
7. Abra o `ControleAcesso.exe`.
8. Realize uma tentativa de acesso.
9. Verifique a notificação exibida no canto inferior direito.

---

## Como compilar para distribuição

1. Selecione o modo `Release` no Visual Studio.
2. Compile a solução.
3. Acesse a pasta:

```text
bin\Release
```

4. Distribua os arquivos necessários junto com o executável.

Arquivos principais:

```text
MonitorAcessoCatraca.exe
MonitorAcessoCatraca.exe.config
Newtonsoft.Json.dll
System.Data.SQLite.dll
Titanium.Web.Proxy.dll
x86\
x64\
.env
```

---

## Observações importantes

- A interceptação HTTPS exige um certificado confiável no Windows.
- O programa deve desativar o proxy do Windows ao ser parado ou fechado.
- O arquivo `.env` não deve ser enviado ao repositório.
- Arquivos de build, logs e bancos locais devem ser ignorados no Git.
- O projeto foi desenvolvido como uma melhoria de usabilidade para clientes que utilizam o Controle de Acesso da Next Fit.

---
