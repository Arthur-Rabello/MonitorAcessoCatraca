Claro. Segue um `README.md` mais bonito, organizado e atualizado para a versão atual do projeto.

Pode substituir o conteúdo do seu README por este. Ele mantém a ideia principal do arquivo que você enviou, mas com uma estrutura mais profissional e limpa. 

````md
# MonitorAcessoCatraca

Monitor de acessos em segundo plano para o **Controle de Acesso da Next Fit**, desenvolvido em **C# Windows Forms (.NET Framework)**.

O sistema acompanha as tentativas de acesso realizadas pelo `ControleAcesso.exe` e exibe uma notificação no canto inferior direito do Windows informando se o acesso foi **liberado** ou **bloqueado**.

---

## 📌 Objetivo

O objetivo do projeto é permitir que a recepção ou o responsável pelo ambiente visualize rapidamente, em forma de pop-up, os acessos realizados na catraca/leitor facial, sem precisar abrir relatórios manualmente.

O monitor funciona em segundo plano, na bandeja do Windows, e acompanha a comunicação realizada pelo Controle de Acesso com a rota responsável pela validação automática.

---

## ⚙️ Funcionamento

Fluxo geral da aplicação:

1. O `MonitorAcessoCatraca` é iniciado.
2. O programa fica em segundo plano na bandeja do Windows.
3. Ele verifica se o processo `ControleAcesso.exe` está aberto.
4. Se necessário, pode tentar abrir o Controle de Acesso automaticamente.
5. O monitor inicia um proxy local para acompanhar a comunicação HTTPS.
6. Quando o Controle de Acesso chama o endpoint de validação automática, a resposta é capturada.
7. A resposta é interpretada pelo sistema.
8. O monitor exibe uma notificação com o resultado do acesso.

---

## 🔔 Notificação exibida

Quando um acesso é capturado, o sistema mostra uma notificação contendo:

- status do acesso;
- nome do cliente;
- horário;
- serviço/contrato, quando disponível;
- motivo do bloqueio ou autorização.

Exemplos de status:

```text
ACESSO LIBERADO
ACESSO BLOQUEADO
````

---

## 🧩 Endpoint monitorado

O endpoint monitorado é configurado via `.env`.

Exemplo:

```env
HOST_ACESSO=acesso.nextfit.com.br
ENDPOINT_ACESSO_AUTOMATICO=ContratoClienteAcesso/AcessoAutomatico
```

A rota monitorada corresponde à validação automática de acesso realizada pelo Controle de Acesso.

---

## ✅ Regra de interpretação

A resposta da validação possui uma estrutura semelhante a:

```json
{
  "Content": {
    "CodigoCliente": 29920661,
    "CodigoContratoCliente": 59956052,
    "Servico": "Musculação",
    "DataValidade": "2026-06-21T03:00:00Z",
    "Erro": false,
    "Mensagem": null,
    "MotivoErro": null
  },
  "Message": "",
  "Success": true
}
```

A regra utilizada pelo monitor é:

```text
Content.Erro = false → acesso liberado
Content.Erro = true  → acesso bloqueado
```

O motivo exibido na notificação é definido nesta ordem:

1. `Content.MotivoErro`
2. `Content.Mensagem`
3. Mensagem padrão do monitor

---

## 👤 Nome do cliente

Quando a resposta possui `CodigoCliente` maior que zero, o monitor tenta buscar o nome do cliente no banco local `banco.db3`.

Banco esperado:

```text
C:\Program Files (x86)\Next Fit\Controle de acesso\banco.db3
```

Quando `CodigoCliente` vem zerado, o monitor tenta extrair o nome a partir da mensagem retornada pela API.

Exemplo:

```text
Mensagem:
Ederaldo Inácio. Acesso inválido, entre em contato com a recepção.

Nome identificado:
Ederaldo Inácio
```

---

## 🖥️ Execução em segundo plano

O programa foi desenvolvido para funcionar em segundo plano:

* fica disponível na bandeja do Windows;
* pode abrir o Controle de Acesso automaticamente;
* exibe notificações no canto inferior direito;
* permite abrir a tela de log pelo ícone da bandeja;
* pode continuar rodando mesmo com a janela principal oculta;
* encerra de verdade pela opção **Sair** no menu da bandeja.

---

## 🔐 Certificado HTTPS

Como a comunicação monitorada é HTTPS, o programa utiliza o `Titanium.Web.Proxy` para interceptar a resposta da requisição.

Na primeira execução, o Windows pode solicitar permissão para confiar no certificado:

```text
Titanium Root Certificate Authority
```

Essa etapa é necessária para que o monitor consiga ler a resposta HTTPS.

Depois que o certificado estiver instalado e confiável, a solicitação não deve aparecer novamente.

---

## 🌐 Proxy do Windows

O monitor utiliza proxy local para acompanhar a comunicação do Controle de Acesso.

A versão atual usa um filtro para descriptografar apenas o host configurado em:

```env
HOST_ACESSO=acesso.nextfit.com.br
```

Ao parar ou encerrar o programa, o proxy do Windows deve ser desativado automaticamente.

O serviço responsável por essa limpeza é:

```text
ProxyWindowsService.cs
```

---

## 📁 Estrutura do projeto

Estrutura recomendada:

```text
MonitorAcessoCatraca
│
├── Config
│   └── AppConfig.cs
│
├── DTOs
│   ├── AcessoAutomaticoResponseDto.cs
│   ├── ConfiguracaoApiDto.cs
│   └── LoginResponseDto.cs
│
├── Forms
│   ├── FormPrincipal.cs
│   └── FormNotificacaoAcesso.cs
│
├── Models
│   └── AcessoAutomatico.cs
│
├── Services
│   ├── AcessoAutomaticoParserService.cs
│   ├── ClienteLocalService.cs
│   ├── ConfiguracaoService.cs
│   ├── NextFitAuthService.cs
│   ├── ProcessoService.cs
│   ├── ProxyInterceptacaoService.cs
│   └── ProxyWindowsService.cs
│
├── Utils
│   ├── JsonHelper.cs
│   └── JwtHelper.cs
│
├── Properties
│   └── AssemblyInfo.cs
│
├── App.config
├── packages.config
├── Program.cs
└── MonitorAcessoCatraca.csproj
```

---

## 📦 Requisitos

* Windows
* Visual Studio
* .NET Framework 4.7.2 ou 4.8
* Controle de Acesso da Next Fit instalado
* Permissão para instalar/confiar certificado local
* Permissão para configurar/desativar proxy do Windows

Caminho padrão do Controle de Acesso:

```text
C:\Program Files (x86)\Next Fit\Controle de acesso
```

Executável esperado:

```text
C:\Program Files (x86)\Next Fit\Controle de acesso\ControleAcesso.exe
```

---

## 📚 Pacotes NuGet

Instale os pacotes abaixo:

```powershell
Install-Package Newtonsoft.Json
Install-Package System.Data.SQLite.Core
Install-Package Titanium.Web.Proxy
```

---

## 🔧 Configuração do `.env`

Crie um arquivo `.env` na raiz do projeto ou junto ao executável.

Conteúdo:

```env
HOST_ACESSO=acesso.nextfit.com.br
ENDPOINT_ACESSO_AUTOMATICO=ContratoClienteAcesso/AcessoAutomatico
```

O arquivo `.env` não deve ser enviado ao Git.

Crie também um `.env.example`:

```env
HOST_ACESSO=acesso.nextfit.com.br
ENDPOINT_ACESSO_AUTOMATICO=ContratoClienteAcesso/AcessoAutomatico
```

---

## 🚫 Arquivos ignorados no Git

Recomenda-se manter no `.gitignore`:

```gitignore
# Visual Studio
.vs/
*.user
*.suo
*.userosscache
*.sln.docstates

# Build
bin/
obj/
Debug/
Release/
x86/
x64/

# NuGet
packages/
*.nupkg

# Logs e debug
debug_*.txt
*.log
*.pdb

# Configurações locais
.env
*.db
*.db3
*.sqlite
*.sqlite3

# Sistema
Thumbs.db
Desktop.ini
```

---

## ▶️ Como executar em desenvolvimento

1. Abra a solução no Visual Studio.
2. Restaure os pacotes NuGet.
3. Crie o arquivo `.env`.
4. Compile o projeto.
5. Execute o `MonitorAcessoCatraca`.
6. Permita a instalação do certificado, se solicitado.
7. Abra o `ControleAcesso.exe`.
8. Faça uma tentativa de acesso.
9. Verifique a notificação exibida no canto inferior direito.

---

## 🏗️ Como compilar para distribuição

1. No Visual Studio, selecione o modo `Release`.
2. Compile a solução.
3. Acesse a pasta:

```text
bin\Release
```

4. Distribua o executável junto com as DLLs necessárias.

Arquivos normalmente necessários:

```text
MonitorAcessoCatraca.exe
MonitorAcessoCatraca.exe.config
Newtonsoft.Json.dll
System.Data.SQLite.dll
Titanium.Web.Proxy.dll
demais DLLs geradas pelo NuGet
x86\
x64\
.env
```

As pastas `x86` e `x64` são importantes para o funcionamento do SQLite.

---

## 📦 Instalador

Para ambiente de cliente, recomenda-se criar um instalador com **Inno Setup**.

O instalador pode:

* copiar os arquivos do programa;
* incluir o `.env`;
* criar atalho;
* configurar inicialização com o Windows;
* solicitar permissão de administrador;
* instalar/confiar o certificado, se necessário;
* executar o monitor ao final da instalação.

---

## ⚠️ Observações importantes

1. A interceptação HTTPS exige certificado confiável no Windows.
2. O monitor depende do `ControleAcesso.exe` gerar a requisição monitorada.
3. O programa deve limpar/desativar o proxy do Windows ao parar ou fechar.
4. O banco local é usado apenas para complementar informações do cliente.
5. Se o endpoint mudar, atualize o `.env`.
6. Se o caminho do Controle de Acesso mudar, atualize `AppConfig.cs`.
7. Arquivos de debug não devem ser enviados ao Git.
8. O arquivo `.env` não deve ser enviado ao repositório.

---

## 🧪 Testes recomendados

Antes de instalar em cliente, teste:

* acesso liberado;
* acesso bloqueado;
* fechamento pelo botão **Sair**;
* limpeza do proxy do Windows;
* abertura automática do Controle de Acesso;
* execução em segundo plano;
* inicialização junto com o Windows;
* funcionamento após reiniciar o computador.

---

## 📝 Versão atual

```text
Versão: 2.0
Modelo: Interceptação HTTPS local otimizada
Interface: Windows Forms
Execução: Segundo plano na bandeja do Windows
Endpoint: Configurado via .env
```

```
```
