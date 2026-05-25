# MonitorAcessoCatraca

Monitor de acessos em segundo plano para o **Controle de Acesso da Next Fit**, desenvolvido em **C# Windows Forms (.NET Framework)**.

O sistema exibe uma notificação no canto inferior direito do Windows sempre que uma tentativa de acesso é identificada, informando se o acesso foi **liberado** ou **bloqueado**.

---

## O que motivou o projeto

O projeto foi desenvolvido a partir de uma solicitação de cliente sobre a necessidade de visualizar de forma mais rápida e clara as tentativas de acesso realizadas no Controle de Acesso.

A funcionalidade foi tratada como uma melhoria de usabilidade para o sistema, permitindo que a recepção ou os responsáveis pelo ambiente recebam uma notificação visual sempre que um acesso for realizado, sem depender da consulta manual de relatórios ou da observação constante da tela principal do Controle de Acesso.

---

## Problemas que o projeto visa resolver

O projeto busca resolver os seguintes pontos:

- dificuldade de acompanhar acessos em tempo real;
- necessidade de consultar relatórios manualmente;
- baixa visibilidade sobre acessos bloqueados;
- ausência de notificação rápida para a recepção;
- melhoria da experiência de uso para clientes que utilizam catraca ou leitor facial.

---

## Como funciona

O `MonitorAcessoCatraca` funciona em segundo plano na bandeja do Windows.

O monitor utiliza um proxy local apenas para identificar quando o `ControleAcesso.exe` realiza uma chamada para a rota de validação automática de acesso.

Quando essa chamada é detectada, o sistema consulta o relatório de acessos da API da Next Fit com limite de 1 registro, buscando apenas o acesso mais recente.

Fluxo resumido:

1. O monitor é iniciado em segundo plano.
2. O sistema verifica se o `ControleAcesso.exe` está aberto.
3. O proxy local identifica uma chamada para `AcessoAutomatico`.
4. O monitor consulta o relatório de acessos com `limit=1`.
5. O último acesso retornado é interpretado.
6. O sistema exibe uma notificação com o resultado do acesso.

---

## Notificação

A notificação pode apresentar:

- nome do cliente;
- status do acesso;
- horário;
- serviço ou contrato, quando disponível;
- motivo do bloqueio, quando houver.

Exemplos:

```text
ACESSO LIBERADO
ACESSO BLOQUEADO
```

---

## Modelo de monitoramento

A versão atual utiliza um modelo híbrido:

| Etapa | Função |
|---|---|
| Proxy local | Detectar quando ocorre a chamada `AcessoAutomatico` |
| Relatório de acesso | Buscar o último acesso registrado com `limit=1` |
| Pop-up | Exibir o resultado para o usuário |

Esse modelo evita consultas constantes ao relatório e reduz a carga na API, pois a consulta só ocorre quando uma validação de acesso é detectada.

---

## Configuração do ambiente

O endpoint monitorado é configurado via arquivo `.env`.

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

## Regra de interpretação

A notificação final é montada a partir do último registro retornado pelo relatório de acessos.

A regra utilizada é baseada no campo de liberação do acesso:

| Valor | Resultado |
|---|---|
| `AcessoLiberado = true` | Acesso liberado |
| `AcessoLiberado = false` | Acesso bloqueado |

O motivo exibido segue esta prioridade:

1. Motivo textual retornado pelo relatório;
2. Tipo de motivo traduzido pelo sistema;
3. Mensagem padrão de acesso bloqueado.

---

## Motivos de bloqueio tratados

O sistema traduz os códigos de motivo de bloqueio para mensagens amigáveis.

| Código | Motivo |
|---|---|
| 0 | Acesso manual |
| 1 | Cliente sem contrato ativo |
| 2 | Contrato sem modalidade |
| 3 | Contrato sem sessão disponível |
| 4 | Contrato sem aula no dia |
| 5 | Contrato fora do dia permitido |
| 6 | Contrato fora do horário permitido |
| 7 | Quantidade de acessos na semana atingida |
| 8 | Quantidade de acessos no período atingida |

---

## Tecnologias utilizadas

| Tecnologia | Utilização |
|---|---|
| C# | Linguagem principal do projeto |
| Windows Forms | Interface gráfica do sistema |
| .NET Framework 4.7.2 / 4.8 | Plataforma de execução |
| Newtonsoft.Json | Manipulação de JSON |
| System.Data.SQLite.Core | Leitura de configurações locais |
| Titanium.Web.Proxy | Detecção da comunicação HTTPS |
| Git | Controle de versão |
| GitHub | Hospedagem do repositório |

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
- O proxy é usado apenas como gatilho para detectar a chamada de acesso automático.
- Os dados exibidos no pop-up são obtidos pelo relatório de acessos com `limit=1`.
- O programa deve desativar o proxy do Windows ao ser parado ou fechado.
- O arquivo `.env` não deve ser enviado ao repositório.
- Arquivos de build, logs, bancos locais e configurações sensíveis devem ser ignorados no Git.
- O projeto foi desenvolvido como uma melhoria de usabilidade para clientes que utilizam o Controle de Acesso da Next Fit.
