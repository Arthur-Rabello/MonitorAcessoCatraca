# MonitorAcessoCatraca

Monitor de acessos em segundo plano para o **Controle de Acesso da Next Fit**, desenvolvido em **C# Windows Forms (.NET Framework 4.7.2)**.

O aplicativo permanece na bandeja do Windows e exibe uma notificação sempre que identifica um novo acesso no relatório da API, informando se ele foi liberado ou bloqueado.

## Como funciona

O monitor não utiliza proxy, interceptação HTTPS ou captura de chamadas do `ControleAcesso.exe`.

Quando o monitoramento está ativo, o aplicativo consulta diretamente o relatório de acessos da API da Next Fit a cada 30 segundos. A primeira consulta é executada imediatamente ao iniciar o monitoramento.

Fluxo:

1. O aplicativo é iniciado em segundo plano.
2. O `ControleAcesso.exe` é verificado periodicamente e pode ser aberto automaticamente.
3. Ao detectar que o Controle de Acesso está aberto, o monitor inicia a leitura do relatório.
4. A cada 30 segundos, a API é consultada com `limit=1`, ordenada pelo campo `DataHora` decrescente.
5. O primeiro registro retornado é considerado o último acesso.
6. O identificador do acesso é comparado com o último acesso notificado.
7. Uma notificação só é exibida quando existe um novo registro.

O intervalo de consulta está definido em `Services/MonitorAcessoServices.cs`:

```csharp
private const int INTERVALO_CONSULTA_MS = 30000;
```

## API consultada

O relatório é consultado diretamente pelo serviço `RelatorioAcessoService`:

```text
GET https://api.nextfit.com.br/api/v2/RelCliente/RecuperarAcessosContrato
```

Parâmetros principais:

- `limit=1`
- `page=1`
- ordenação por `DataHora` decrescente
- janela padrão de 3 minutos
- `TipoAcesso=1`
- `AgruparAcessosPorCliente=false`
- `ExibirClientesAgregadores=true`

O token Bearer e o código da unidade são obtidos pelo login na API antes da consulta.

## Notificação

A notificação pode apresentar:

- nome do cliente;
- status do acesso;
- horário do acesso;
- contrato ou serviço;
- motivo do bloqueio, quando informado.

Exemplos:

```text
ACESSO LIBERADO
ACESSO BLOQUEADO
```

O sistema também registra o acesso no log da tela principal.

## Regra de deduplicação

O monitor armazena o identificador do último acesso notificado em memória. Se a consulta seguinte retornar o mesmo identificador, nenhuma nova notificação é exibida.

O identificador é obtido nesta ordem:

1. `Id` do registro do relatório;
2. `CodigoContratoClienteAcesso`, caso `Id` não esteja disponível.

O monitor consulta o relatório mesmo quando não há um novo acesso, mas somente notifica registros ainda não processados.

## Interpretação do acesso

A decisão de liberação utiliza o campo `AcessoLiberado`:

| Valor | Resultado |
|---|---|
| `true` | Acesso liberado |
| `false` | Acesso bloqueado |

O motivo exibido segue esta prioridade:

1. Motivo textual retornado pela API;
2. Tipo de motivo traduzido pelo sistema;
3. Mensagem padrão de acesso autorizado ou bloqueado.

## Motivos de bloqueio tratados

| Código | Motivo |
|---:|---|
| 0 | Acesso manual |
| 1 | Cliente sem contrato ativo |
| 2 | Contrato sem modalidade |
| 3 | Contrato sem sessão disponível |
| 4 | Contrato sem aula no dia |
| 5 | Contrato fora do dia permitido |
| 6 | Contrato fora do horário permitido |
| 7 | Quantidade de acessos na semana atingida |
| 8 | Quantidade de acessos no período atingida |

## Autenticação e configuração

O login é realizado diretamente na API da Next Fit:

```text
POST https://api.nextfit.com.br/api/token/
```

As credenciais são lidas da tabela `CONFIGURACAO` do banco local:

```text
C:\Program Files (x86)\Next Fit\Controle de acesso\banco.db3
```

O banco deve conter os campos:

- `EMAIL`
- `SENHA`

Após o login, o código da unidade é extraído do token JWT. Caso não seja encontrado, o sistema utiliza o código `1` como fallback.

O aplicativo também exige um arquivo `.env` para carregar as configurações de identificação exibidas na interface:

```env
HOST_ACESSO=https://api.nextfit.com.br
ENDPOINT_ACESSO_AUTOMATICO=/api/v2/RelCliente/RecuperarAcessosContrato
```

O `.env` deve ficar junto ao executável ou em um dos diretórios procurados por `AppConfig`. Ele não deve ser enviado ao Git.

## Controle do aplicativo

O monitor possui:

- execução na bandeja do Windows;
- inicialização com o Windows;
- botão para iniciar o monitoramento;
- botão para parar o monitoramento;
- abertura automática do Controle de Acesso;
- prevenção de múltiplas instâncias por mutex global;
- atalho `F9` para liberação manual de acesso.

Ao fechar a janela, o aplicativo permanece na bandeja. Para encerrar completamente, use a opção de saída no menu da bandeja.

## Tecnologias utilizadas

| Tecnologia | Utilização |
|---|---|
| C# | Linguagem principal |
| Windows Forms | Interface gráfica |
| .NET Framework 4.7.2 | Plataforma de execução |
| `HttpClient` | Comunicação com a API |
| `Newtonsoft.Json` | Desserialização das respostas JSON |
| `System.Data.SQLite.Core` | Leitura das credenciais locais |
| Git | Controle de versão |

O projeto ainda possui uma referência legada ao `Titanium.Web.Proxy`, mas o fluxo atual de monitoramento não utiliza proxy nem interceptação de tráfego.

## Pacotes NuGet

As referências atuais do projeto estão em `packages.config`. Entre os pacotes utilizados estão:

```powershell
Install-Package Newtonsoft.Json
Install-Package System.Data.SQLite.Core
```

As dependências existentes devem ser restauradas pelo Visual Studio antes da compilação. A referência legada do `Titanium.Web.Proxy` pode ser removida do projeto em uma limpeza futura, caso não seja necessária por outras partes da solução.

## Estrutura principal

```text
MonitorAcessoCatraca
├── Config
├── DTOs
├── Enums
├── Forms
├── Models
├── Services
├── Utils
├── App.config
├── packages.config
├── Program.cs
└── MonitorAcessoCatraca.csproj
```

Principais serviços:

- `MonitorAcessoService`: ciclo de vida, timer e notificações;
- `RelatorioAcessoService`: consulta e interpretação do relatório;
- `NextFitAuthService`: login e token da API;
- `ConfiguracaoService`: leitura das credenciais no SQLite;
- `ProcessoService`: verificação e abertura do Controle de Acesso;
- `AcessoManualService`: liberação manual pelo atalho `F9`.

## Como executar

1. Abra a solução no Visual Studio.
2. Restaure os pacotes NuGet.
3. Confirme a existência do banco do Controle de Acesso.
4. Configure as credenciais na tabela `CONFIGURACAO`.
5. Crie o arquivo `.env` com as variáveis necessárias.
6. Compile o projeto.
7. Execute o `MonitorAcessoCatraca`.
8. Inicie o monitoramento pela interface ou pela bandeja.
9. Verifique as notificações e o log da aplicação.

Não é necessário instalar certificado, configurar proxy ou interceptar o tráfego do Controle de Acesso.

## Compilação para distribuição

1. Selecione o modo `Release` no Visual Studio.
2. Compile a solução.
3. Acesse:

```text
bin\Release
```

Distribua o executável com as DLLs geradas, o arquivo `.env` e os arquivos de configuração necessários. Nunca distribua credenciais em um repositório público.

## Observações importantes

- A consulta ocorre a cada 30 segundos enquanto o monitoramento estiver ativo.
- A primeira consulta ocorre imediatamente ao iniciar o monitoramento.
- O sistema depende da disponibilidade da API da Next Fit e de um token válido.
- A janela padrão consultada pelo relatório é de 3 minutos.
- O monitor não observa diretamente a catraca nem o processo do Controle de Acesso para detectar eventos individuais.
- O relatório pode não retornar um acesso se ele estiver fora da janela de consulta configurada.
- A notificação depende de o registro retornar com um identificador válido.
- O monitor não utiliza proxy local ou certificado HTTPS.
- O arquivo `.env`, o banco SQLite, logs e demais dados sensíveis não devem ser versionados.
