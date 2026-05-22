MonitorAcessoCatraca - Monitor de Acessos em Segundo Plano

DESCRIÇÃO
----------
O MonitorAcessoCatraca é um aplicativo Windows Forms em C# desenvolvido para monitorar tentativas de acesso realizadas pelo Controle de Acesso da Next Fit.

O programa funciona em segundo plano, na bandeja do Windows, e exibe uma notificação no canto inferior direito sempre que uma tentativa de acesso for identificada.

A versão atual monitora a comunicação HTTPS feita pelo ControleAcesso.exe com o endpoint:

https://acesso.nextfit.com.br/api/v1/ContratoClienteAcesso/AcessoAutomatico

A resposta dessa requisição é utilizada para identificar se o acesso foi liberado ou bloqueado.


FUNCIONAMENTO
-------------
O fluxo do sistema é:

1. O MonitorAcessoCatraca é iniciado.
2. O programa fica em segundo plano na bandeja do Windows.
3. Ele verifica se o processo ControleAcesso.exe está aberto.
4. Se o ControleAcesso.exe não estiver aberto, o monitor tenta abri-lo automaticamente.
5. O monitor inicia um proxy local para acompanhar a comunicação HTTPS do Controle de Acesso.
6. Quando o Controle de Acesso chama o endpoint AcessoAutomatico, o monitor captura a resposta.
7. A resposta é convertida para um modelo interno.
8. Quando necessário, o nome do cliente é buscado no banco local banco.db3.
9. Uma notificação é exibida no canto inferior direito do computador.


REQUISITOS
----------
- Windows
- .NET Framework 4.7.2 ou 4.8
- Visual Studio com suporte a Windows Forms
- Permissão de administrador para instalação/configuração do certificado HTTPS local
- Controle de Acesso da Next Fit instalado em:

  C:\Program Files (x86)\Next Fit\Controle de acesso

- Banco local esperado em:

  C:\Program Files (x86)\Next Fit\Controle de acesso\banco.db3


PACOTES NUGET NECESSÁRIOS
-------------------------
Instale os seguintes pacotes no projeto:

- Newtonsoft.Json
- System.Data.SQLite.Core
- Titanium.Web.Proxy

Pelo Console do Gerenciador de Pacotes NuGet:

Install-Package Newtonsoft.Json
Install-Package System.Data.SQLite.Core
Install-Package Titanium.Web.Proxy


ESTRUTURA DO PROJETO
--------------------
Estrutura recomendada:

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
│   └── ProxyInterceptacaoService.cs
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


CONFIGURAÇÃO PRINCIPAL
----------------------
As configurações fixas ficam em:

Config\AppConfig.cs

Esse arquivo centraliza:

- pasta do Controle de Acesso;
- caminho do banco.db3;
- nome do processo ControleAcesso;
- nome do executável ControleAcesso.exe;
- host monitorado;
- endpoint monitorado;
- porta do proxy local.

Configurações principais:

Pasta do Controle de Acesso:
C:\Program Files (x86)\Next Fit\Controle de acesso

Banco local:
C:\Program Files (x86)\Next Fit\Controle de acesso\banco.db3

Executável:
C:\Program Files (x86)\Next Fit\Controle de acesso\ControleAcesso.exe

Processo:
ControleAcesso

Host monitorado:
acesso.nextfit.com.br

Endpoint monitorado:
/api/v1/ContratoClienteAcesso/AcessoAutomatico


CERTIFICADO HTTPS
-----------------
Como a comunicação monitorada é HTTPS, o programa utiliza um proxy local para conseguir ler a resposta da requisição.

Para isso, é necessário instalar/confiar em um certificado raiz local gerado pelo Titanium.Web.Proxy.

Na primeira execução, o Windows pode exibir uma solicitação para confiar no certificado:

Titanium Root Certificate Authority

Essa etapa é necessária para que o monitor consiga ler a resposta da requisição HTTPS.

Depois que o certificado estiver instalado e confiável, a solicitação não deve aparecer novamente em execuções futuras.


EXECUÇÃO EM SEGUNDO PLANO
-------------------------
O programa foi feito para funcionar em segundo plano:

- inicia e fica disponível na bandeja do Windows;
- pode abrir o ControleAcesso.exe automaticamente;
- monitora a comunicação do Controle de Acesso;
- exibe notificações no canto inferior direito;
- permite abrir a tela de log pelo ícone da bandeja;
- ao clicar no X, a janela pode ser ocultada sem encerrar o monitor;
- para encerrar de verdade, use a opção Sair no menu da bandeja.


CONTROLE DE ACESSO
------------------
O monitor verifica se o processo abaixo está aberto:

ControleAcesso.exe

No C#, o processo é identificado como:

ControleAcesso

Se o processo não estiver aberto, o monitor tenta executar:

C:\Program Files (x86)\Next Fit\Controle de acesso\ControleAcesso.exe

A abertura automática pode ser limitada para ocorrer apenas uma vez, evitando que o monitor fique reabrindo o Controle de Acesso caso o usuário feche manualmente.


ENDPOINT MONITORADO
-------------------
Endpoint interceptado:

https://acesso.nextfit.com.br/api/v1/ContratoClienteAcesso/AcessoAutomatico

Exemplo de resposta com acesso liberado:

{
  "Content": {
    "CodigoCliente": 29920661,
    "CodigoContratoCliente": 59956052,
    "DataValidade": "2026-06-21T03:00:00Z",
    "Erro": false,
    "Mensagem": null,
    "MotivoErro": null,
    "ProximoValorReceber": null,
    "ProximoVencimentoReceber": null,
    "Servico": "Musculação"
  },
  "Message": "",
  "Success": true
}

Exemplo de resposta com acesso bloqueado:

{
  "Content": {
    "CodigoCliente": 0,
    "CodigoContratoCliente": null,
    "DataValidade": "0001-01-01T03:06:00Z",
    "Erro": true,
    "Mensagem": "Ederaldo Inácio. Acesso inválido, entre em contato com a recepção.",
    "MotivoErro": "Cliente sem contrato ativo",
    "ProximoValorReceber": null,
    "ProximoVencimentoReceber": null,
    "Servico": null
  },
  "Message": "",
  "Success": true
}


REGRA DE INTERPRETAÇÃO
----------------------
A regra usada pelo monitor é:

Content.Erro = false
→ acesso liberado

Content.Erro = true
→ acesso bloqueado

O motivo exibido na notificação é definido nesta ordem:

1. Content.MotivoErro
2. Content.Mensagem
3. Mensagem padrão do monitor


NOME DO CLIENTE
---------------
Quando a resposta contém CodigoCliente maior que zero, o monitor tenta buscar o nome do cliente no banco local banco.db3, na tabela CLIENTES.

Quando CodigoCliente vem zerado, o monitor tenta extrair o nome a partir de Content.Mensagem.

Exemplo:

Mensagem:
Ederaldo Inácio. Acesso inválido, entre em contato com a recepção.

Nome identificado:
Ederaldo Inácio


NOTIFICAÇÃO
-----------
Quando um acesso é capturado, o sistema exibe uma notificação com:

- status do acesso;
- nome do cliente;
- horário;
- serviço, quando disponível;
- motivo do bloqueio ou autorização.

Exemplos de status:

ACESSO LIBERADO
ACESSO BLOQUEADO


COMO COMPILAR
-------------
1. Abra a solução no Visual Studio.
2. Confirme que o projeto está em .NET Framework 4.7.2 ou 4.8.
3. Restaure os pacotes NuGet.
4. Compile em modo Release.
5. Acesse a pasta:

   bin\Release

6. Distribua a pasta Release com todas as DLLs necessárias.


ARQUIVOS NECESSÁRIOS PARA DISTRIBUIÇÃO
--------------------------------------
Na pasta Release, normalmente devem ser incluídos:

- MonitorAcessoCatraca.exe
- MonitorAcessoCatraca.exe.config
- Newtonsoft.Json.dll
- System.Data.SQLite.dll
- Titanium.Web.Proxy.dll
- demais DLLs geradas pelo NuGet
- pasta x86
- pasta x64

As pastas x86 e x64 são importantes para o funcionamento do SQLite, pois podem conter DLLs nativas necessárias.


INSTALADOR
----------
Para ambiente de cliente, recomenda-se criar um instalador com Inno Setup.

O instalador pode:

- copiar os arquivos do programa;
- criar atalho;
- configurar inicialização com o Windows;
- solicitar permissão de administrador;
- instalar o certificado, se necessário;
- executar o monitor ao final da instalação.


OBSERVAÇÕES IMPORTANTES
-----------------------
1. A interceptação HTTPS exige certificado confiável no Windows.
2. O programa deve ser executado com permissões adequadas para configurar o proxy local.
3. O monitor depende do ControleAcesso.exe para gerar a requisição monitorada.
4. Se o endpoint da Next Fit mudar, atualize AppConfig.EndpointAcessoAutomatico.
5. Se o executável do Controle de Acesso mudar de nome, atualize AppConfig.NomeProcessoControleAcesso e AppConfig.NomeExecutavelControleAcesso.
6. O banco local é usado apenas para complementar informações do cliente quando necessário.


VERSÃO ATUAL
------------
Versão: 2.0
Modelo de monitoramento: interceptação HTTPS local
Endpoint monitorado: /api/v1/ContratoClienteAcesso/AcessoAutomatico
Execução: segundo plano na bandeja do Windows