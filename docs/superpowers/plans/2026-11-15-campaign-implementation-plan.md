# Campaign Voyage MVP Implementation Plan

**Status:** Demonstração mínima do MVP aceita pelo usuário em 2026-11-16; próximo incremento é acabamento de apresentação mobile.
**Execução:** Native, nesta sessão.
**Data:** 2026-11-16

**Objetivo:** Implementar o loop de campanha de três relíquias em uma Activity, mantendo as regras do jogo independentes da Unity e usando apenas a composição de Framework necessária para o boot.

**Arquitetura:** Uma Game Application inicia uma Route de campanha com uma Activity de campanha. A cena de gameplay é a cena primária da Route e o startup Activity é a Activity da campanha; como ambos têm a mesma duração neste MVP de uma única cena, a Activity não precisa de Activity Content Profile. Um assembly puro possui definições e simulação. Um assembly Unity adapta dados de configuração, ciclo de frame, toque e apresentação. O Framework continua dono do fluxo amplo e da sessão da câmera; o jogo é dono da campanha, da tentativa, do navio, dos macacos e das recompensas.

**Tecnologia:** Unity 6000.6.9f1; C#; Input System 1.20.1 já resolvido; uGUI 2.6.0; NUnit pretendido para testes EditMode da lógica pura. A cópia local do Immersive Framework declara Cinemachine 3.1.7, enquanto o lock resolve Cinemachine 6.6.0 builtin. Não alterar Manifest, Lock ou código-fonte dos pacotes para contornar incompatibilidade.

**Documentos aprovados:** [GDD Voyage System](../../../design/gdd/voyage-system.md), [Activity de campanha](../specs/2026-11-15-campaign-activity-design.md) e [contrato de boot do Framework](../../../design/architecture/framework-application-boot-contract.md).

## Status atual da execução (2026-11-16)

- O Unity CLI 1.0.0 alcança o Editor aberto 6000.6.9f1 em estado ready. A recompilação concluiu com zero erros de C#; o estado atual informa compilação concluída e zero erros ativos. O Console reporta dois avisos atuais.
- A composição Framework foi criada pelo Editor: PersistentContent e CampaignActivity; Game Application, CampaignRoute e CampaignActivity com IDs válidos; CameraOutputDefinition, FixedCameraRigBehaviorDefinition e CameraOutput.prefab. A Settings aponta para a aplicação. Player Session e Progression Save permanecem desligados.
- Apply / Rebuild materializou a CinemachineCamera e retornou ApplySucceeded. Project Settings/Game Application, Model Readiness, Camera Session e Persistent Content template validam sem erros ou avisos.
- SampleScene permanece intacta e ativa. PersistentContent e CampaignActivity seguem habilitadas na lista global de cenas. O alvo ativo é Android e o Build Profile `Android` foi criado e ativado pelo Unity CLI; nenhum APK foi gerado.
- O mismatch declarado/resolvido do Cinemachine permanece: Framework local declara 3.1.7 e o lock resolve 6.6.0. A compilação e validação de autoria não comprovam comportamento em runtime; não alterar Manifest, Lock ou pacote para contorná-lo.
- O usuário apresentou uma captura do Unity Test Runner com 22/22 testes NUnit EditMode aprovados. O usuário também confirmou game over, conclusão da campanha e diferentes configurações; os logs mais recentes mostram HUD em `Sailing`, spawn de dois macacos e derrota correta das classes de um e dois cliques.
- A cena agora usa marcadores de relíquia criados a partir de prefab conforme `RequiredRelics`; a campanha de teste usa três viagens de 60 segundos. O asset de produção permanece separado.
- O baseline de produção aprovado é configurável entre 60–300 segundos, com duração inicial de 300. A campanha real mantém seus dados próprios; os valores reduzidos estão isolados no asset de teste.
- O package manifest aponta para o checkout local 1.2.0 por caminho relativo; a resolução preserva Composition 0.1.0-preview.3, Foundation 0.2.3 e Logging 0.2.4. A diferença de Cinemachine 3.1.7 declarada e 6.6.0 resolvida permanece sem validação específica de runtime.
- Nenhum APK foi gerado nem testado em aparelho. O próximo corte de produto é corrigir legibilidade e layout responsivo da HUD; direção de arte e substituição dos visuais temporários vêm depois.
- O Git Bash instalado falha dentro do sandbox padrão com `NtCreateDirectoryObject 0xC0000022`, mas `bash.exe --version` funciona em execução elevada. `CGS_BASH` pode selecionar o binário por processo; não foi alterada nenhuma permissão global.

## Aditivo de integração UX aprovado (2026-11-17)

A baseline aprovada em `design/ux/campaign-flow.md` substitui as decisões anteriores deste plano sobre pausa local e Retry direto. O Framework é a autoridade para Pause e para o ciclo de Activity; o jogo mantém as regras da tentativa e a semântica da recompensa.

- O controller consulta um `PauseSnapshot` válido antes de avançar a viagem, aceitar toques ou emitir Continue. `OnApplicationPause(true)` encaminha um pedido ao `PauseRequestTrigger`; o jogo não mantém uma segunda variável de pausa nem altera `Time.timeScale`.
- A sessão da campanha permanece no conteúdo da Route. O controller é um participante `IUnityResettable` sob um `Resettable` com membership `Activity`. Retry chama `ActivityRestartTrigger` com alvo `CurrentActivity`; a regra de Retry executa no Reset, preservando relíquias e índice, e a admissão só é liberada após sucesso terminal. Falhas permanecem bloqueadas e diagnosticadas.
- A implementação desta sessão conecta controller, HUD e assembly às APIs do Framework. A autoria no Unity Editor ainda precisa adicionar/validar Pause Request Trigger, Activity Restart Trigger e Resettable no objeto Campaign Runtime, com membership Activity e alvo Current Activity.
- A superfície visual de pausa, Android Back, menu/rotas e loading/fade permanecem nos próximos cortes. O projeto atual ainda não tem Route de menu para receber as transições compostas.

## Registro da execução inicial (2026-11-15)

- Foram escritos o domínio puro de viagem/campanha, os assemblies, fontes NUnit e adaptadores de dados, toque, controller, board, HUD e catálogo localizado. Eles ainda não foram compilados nem testados no Unity.
- O Unity CLI 1.0.0 está instalado. O Editor 6000.6.9f1 foi detectado para este projeto, mas o CLI o reportou como inacessível; `recompile` falhou ao acessar o socket local (Windows error 10013). Nenhum comando trouxe a janela para frente.
- O Editor gerou `.meta` para as novas pastas, mas nenhum assembly compilado foi observado. Não considerar a importação/compilação aprovada.
- Cenas, prefabs e assets serializados do Framework continuam pendentes. Como o Editor está aberto e o CLI não responde, esses arquivos não foram editados manualmente.
- A definição de campanha de produção e a cena ainda precisam ser geradas pelo Unity Editor; validação visual e em aparelho permanece pendente.
- A verificação PreToolUse/PostToolUse do repositório não iniciou: o launcher não consegue executar o Bash empacotado, com `NtCreateDirectoryObject 0xC0000022`. O seletor `rules.mjs` não cobre o Unity aninhado sob `Games/...`; regras nativas de gameplay e testes foram lidas manualmente.

## Restrições globais

- Preservar as alterações já existentes em Packages/manifest.json e Packages/packages-lock.json; o caminho local do Framework não é configuração portátil.
- Não alterar Packages nem os pacotes do Immersive Framework. Não criar Player, PlayerInput, serviço global, singleton, bootstrap próprio, FindObjectOfType ou authority paralela para fluxo, input, câmera ou pause.
- Usar assembly Gameplay puro com noEngineReferences true; manter qualquer dependência de Unity, Input System e uGUI no assembly adaptador.
- Usar a UI action map já existente, em especial Point e Click. Não habilitar a Player action map, que contém bindings de gamepad, nem adicionar gamepad.
- A UI do tabuleiro será Canvas em Screen Space Overlay. O toque entra pelo InputSystemUIInputModule e uma superfície de tabuleiro; o domínio decide qual macaco derrotar. O Output de câmera do Framework ainda é obrigatório para o boot, mas não será usado para converter coordenadas da tela do tabuleiro.
- A pausa por segundo plano será local à simulação do jogo via OnApplicationPause. Não alterar Time.timeScale nem duplicar o Framework Pause.
- O usuário aceitou para o MVP Camera Session, CameraOutputDefinition e FixedCameraRigBehaviorDefinition Experimentais. Usar somente o mínimo auditado, sem alegar estabilidade ou compatibilidade ainda não verificada.
- Cada viagem é configurável entre 180 e 300 segundos (inicial 300). O baseline de afundamento, cronogramas, raios e áreas aleatórias foi aprovado e é autorável no Inspector. Os valores 10 s e k = 0,1 são apenas fixtures matemáticos do GDD.
- O usuário aprovou um painel de resultado sobre a mesma cena da Activity, com Retry ou Continue conforme o estado; nenhuma Activity ou cena adicional de resultado será criada.
- Sem arte final, áudio, save persistente, alvo de sistema operacional móvel ou aparelhos de desempenho nesta fatia.

## Decisões aprovadas

1. O painel de resultado permanece sobre a cena da Activity.
2. O domínio puro recebe Point2; adaptadores Unity convertem toque e apresentam snapshots.
3. A composição mínima de câmera Experimental aceita será usada sem modificar o Framework.
4. Os valores de afundamento, ondas, áreas e raios aprovados são dados de autor no asset de campanha.
5. Cada viagem dura de 180 a 300 segundos; as ondas preservam quantidade e classe, enquanto seus horários e posições são sorteados dentro de limites configurados. Retry repete a amostra da sessão.
6. O alvo Unity ativo é Android, com Build Profile `Android`; build de APK ainda não foi executado.

## Etapa 0: Pré-condição de importação do Unity

**Estado em 2026-11-16:** a importação e recompilação foram observadas pelo Unity CLI no Editor aberto; a compilação terminou sem erros de C#. Os validadores de autoria da Camera Session e do prefab também passaram. O Console ainda tem dois avisos atuais.

A dependência local declara Cinemachine 3.1.7, enquanto o lock resolve 6.6.0. A compilação e validação de assets não demonstram compatibilidade em runtime. Não editar Manifest, Lock ou o pacote para mascarar a diferença; a confirmação de comportamento continua pendente.

## Etapa 1: Criar a composição mínima de aplicação

**Assets Unity previstos:**

- Criar Assets/_Project/Scenes/PersistentContent.unity e Assets/_Project/Scenes/CampaignActivity.unity. Manter Assets/Scenes/SampleScene.unity sem uso e sem alterações nesta fatia.
- Criar GameApplicationAsset, CampaignRoute RouteAsset e CampaignActivity ActivityAsset em Assets/_Project/Configuration/ImmersiveFramework/.
- Criar CameraOutputDefinition e FixedCameraRigBehaviorDefinition em Assets/_Project/Configuration/ImmersiveFramework/Camera/ e um prefab CameraOutput.prefab em Assets/_Project/Prefabs/ImmersiveFramework/.
- Atualizar Assets/_Project/Settings/ImmersiveFramework/Resources/ImmersiveFrameworkSettings.asset para apontar à Game Application e à Camera Session configurada; manter Player Session e Progression Save desligados e frame rate em UseUnityDefaults.

**Procedimento no Editor:**

1. Usar Persistent Content Scene Template e validar seu escopo; habilitar CampaignActivity.unity e PersistentContent.unity no Build Profile.
2. Configurar CampaignActivity.unity como cena primária da Route e CampaignActivity como Startup Activity. Deixar Activity Content Profile vazio porque não há cena adicional nem segunda Activity no MVP.
3. Atribuir IDs válidos e únicos à Route e à Activity.
4. Configurar um prefab de Output com exatamente um CameraOutputAuthoring, Unity Camera, CinemachineBrain e Fallback Camera Rig explícitos. Atribuir CameraOutputDefinition e o comportamento fixo Experimental.
5. Em Edit Mode, usar Apply / Rebuild no CameraRigComposer; salvar o prefab após materializar a CinemachineCamera. Evitar câmera física ativa duplicada na composição.
6. Atribuir a Camera Session no Settings e executar os validadores descritos no contrato de boot.

**Referência canônica:** planet-devourer, Assets/_Sample/GameFlow/GameApplication_GameFlow.asset tem Player Session desligada, uma câmera de Output, nenhuma Startup Camera Assignment e Persistent Content; Assets/_Sample/GameFlow/GameFlowShowcase/Routes/Route_BasicFlow.asset aponta para cena primária e Startup Activity; o README confirma Activity sem Activity Content Profile. O Git metadata dessa cópia não pôde ser lido, mas os arquivos do sample foram inspecionados.

**Aceitação:** Game Application aponta para a Route; a Route abre uma Activity na cena primária; Persistent Content e cenas estão no Build Profile; todos os validadores de autoria passam. Nenhuma chamada a API interna de bootstrap é adicionada ao código do jogo.

## Etapa 2: Criar o domínio puro da campanha e da viagem

**Criar arquivos:**

- Assets/_Project/Scripts/Gameplay/MacacosMeMordam.Gameplay.asmdef
- Assets/_Project/Scripts/Gameplay/Point2.cs
- Assets/_Project/Scripts/Gameplay/MonkeyClassDefinition.cs
- Assets/_Project/Scripts/Gameplay/MonkeySpawnDefinition.cs
- Assets/_Project/Scripts/Gameplay/SpawnWaveDefinition.cs
- Assets/_Project/Scripts/Gameplay/VoyageDefinition.cs
- Assets/_Project/Scripts/Gameplay/CampaignDefinition.cs
- Assets/_Project/Scripts/Gameplay/VoyageState.cs
- Assets/_Project/Scripts/Gameplay/MonkeySnapshot.cs
- Assets/_Project/Scripts/Gameplay/VoyageSnapshot.cs
- Assets/_Project/Scripts/Gameplay/IVoyageSimulation.cs
- Assets/_Project/Scripts/Gameplay/VoyageSimulation.cs
- Assets/_Project/Scripts/Gameplay/CampaignState.cs
- Assets/_Project/Scripts/Gameplay/CampaignSnapshot.cs
- Assets/_Project/Scripts/Gameplay/ICampaignSession.cs
- Assets/_Project/Scripts/Gameplay/CampaignSession.cs

**Contratos propostos:**

- Gameplay.asmdef define noEngineReferences true e não referencia Foundation ou outro package apenas para obter primitives genéricas.
- Point2 é um valor de coordenadas independente de Unity. MonkeyClassDefinition define classe, peso positivo e raio de acerto positivo. SpawnWaveDefinition define progresso de disparo em [0, 1], ordem e composição ordenada. VoyageDefinition contém duração positiva e ondas ordenadas. CampaignDefinition contém viagens na ordem da campanha, taxa de afundamento positiva e limiar de relíquias igual a três.
- TryCreate/TryValidate rejeitam dados inválidos com erros diagnósticos; a Activity não inicia simulação se a configuração falhar. A campanha precisa conter pelo menos três cronogramas para que cada sucesso antes do final possa avançar. Não inferir identidade de strings nem comparar IDs de domínios diferentes.
- VoyageSimulation.Advance(deltaSeconds) consome o tempo ativo até o limite solicitado e processa em ordem os próximos eventos de surgimento, afundamento e chegada. O primeiro evento terminal encerra o passo no instante correto. Chegada vence empate exato com afundamento; afundamento anterior vence chegada posterior. Ondas em 100% não surgem após chegada.
- VoyageSimulation.TryDefeatAt(point) considera apenas macacos ativos e dentro do raio; derrota no máximo um. Ordena por distância ao quadrado e depois por ordem de surgimento crescente. Remover carga não desfaz afundamento já acumulado.
- CampaignSession.Start inicia o cronograma zero. Ao chegar, concede uma relíquia uma vez; com menos de três entra em ReadyForContinue e avança o índice uma vez. Continue inicia esse próximo índice sem recompensa. Retry após Sunk reinicia somente a tentativa e repete o índice atual. Na terceira relíquia entra em CurseBroken sem próxima viagem.
- CampaignSession publica um snapshot imutável para apresentação; objetos Unity não são armazenados no domínio. As APIs recebem tempo explícito e não leem Time, Input, GameObject ou cena.

**Aceitação:** Cada regra aprovada do GDD pode ser executada por chamadas explícitas ao domínio sem Unity. Chamadas após estado terminal não alteram a tentativa nem concedem outra relíquia.

## Etapa 3: Cobrir o domínio com NUnit EditMode

**Criar arquivos:**

- Assets/Tests/EditMode/MacacosMeMordam.Gameplay.Tests.asmdef
- Assets/Tests/EditMode/VoyageTestFixtures.cs
- Assets/Tests/EditMode/VoyageSimulationTests.cs
- Assets/Tests/EditMode/CampaignSessionTests.cs

O assembly de teste referencia Gameplay e Test Framework, sem referenciar os adaptadores Unity.

**Asserções derivadas do GDD:** chegada sem carga; sinking anterior à chegada; empate chegada/afundamento com uma recompensa; acumulação de carga leve/pesada e ausência de reversão; seleção do mais próximo e desempate pelo surgimento mais antigo; onda em 100% suprimida; tentativa terminal imutável; pausa representada por nenhum Advance; Retry mantém relíquias e índice; Continue avança exatamente uma vez e não recompensa; terceira relíquia encerra a campanha. Usar fixture do GDD de 10 segundos e k = 0,1 apenas nestes testes, nunca como default da campanha.

**Aceitação:** cada cenário tem estado inicial, comando/tempo e resultado assertivo; não há teste de integração com a Unity nesta fatia. A execução dos testes ocorre somente após autorização explícita para testar/verificar a implementação.

## Etapa 4: Adaptar configuração, toque e apresentação

**Criar arquivos:**

- Assets/_Project/Scripts/Unity/MacacosMeMordam.Unity.asmdef
- Assets/_Project/Scripts/Unity/CampaignDefinitionAsset.cs
- Assets/_Project/Scripts/Unity/CampaignLocalizationAsset.cs
- Assets/_Project/Scripts/Unity/CampaignTextKey.cs
- Assets/_Project/Scripts/Unity/CampaignActivityController.cs
- Assets/_Project/Scripts/Unity/CampaignTouchBoard.cs
- Assets/_Project/Scripts/Unity/CampaignBoardView.cs
- Assets/_Project/Scripts/Unity/CampaignHudView.cs
- Assets/_Project/Scripts/Unity/MonkeyView.cs

**Responsabilidades:**

- Unity.asmdef referencia Gameplay, Unity.InputSystem e UnityEngine.UI segundo os asmdefs realmente instalados; Unity framework types não vazam para Gameplay.
- CampaignDefinitionAsset serializa os dados aprovados e converte para definições validadas. Ausência ou erro bloqueia início com mensagem diagnóstica, sem valores substitutos.
- CampaignActivityController é composição local da cena, recebe referências serializadas aos assets/views, cria uma CampaignSession e avança com Time.deltaTime quando ativa. OnApplicationPause bloqueia Advance enquanto suspenso e preserva snapshot; não altera Time.timeScale.
- CampaignTouchBoard implementa IPointerClickHandler no Canvas Screen Space Overlay, converte PointerEventData.position com RectTransformUtility.ScreenPointToLocalPointInRectangle e envia Point2 ao domínio. Usar câmera null para o canvas Overlay. Os macacos não resolvem toque nem removem a si mesmos.
- CampaignLocalizationAsset fornece texto localizado por chaves explícitas e falha se faltar um idioma ou uma entrada; o HUD não embute cópia de interface.
- InputSystemUIInputModule usa as ações Point e Click da UI map existente. Botões Retry e Continue apenas enviam comandos à CampaignSession quando o snapshot permite. Não habilitar Player map, PlayerInput ou gamepad.
- BoardView, HudView e MonkeyView apenas desenham o snapshot; não calculam peso, sink, seleção, recompensa ou cronograma. Visual provisório com formas/UI básicas.

**Referências de API:** [Unity OnApplicationPause](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/MonoBehaviour.OnApplicationPause.html) e [ScreenPointToLocalPointInRectangle](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/RectTransformUtility.ScreenPointToLocalPointInRectangle.html).

## Etapa 5: Integrar a cena e o fluxo visual

Configurar CampaignActivity.unity com EventSystem/InputSystemUIInputModule, Canvas Overlay, superfície de toque, HUD, painel de resultado e referências locais ao controller. O painel apresenta Sailing, Arrived, Sunk e CurseBroken. Retry aparece somente após Sunk; Continue aparece somente em ReadyForContinue; a conclusão da campanha não oferece nova viagem. A chegada com menos de três relíquias mantém o painel aberto até Continue.

O painel de resultado sobre a mesma cena da Activity está aprovado. Não criar tela separada nem transição adicional de Framework nesta implementação.

**Estado do balanceamento:** o baseline aprovado está em `design/quick-specs/2026-11-16-android-voyage-balance-proposal.md`. Criar `CampaignDefinition.asset` pelo Editor com duração 180–300 s, `k=0.0015`, cronogramas aprovados e variação temporal/espacial. Não introduzir fallback.

## Etapa 6: Demonstração e validação restante

**Demonstração mínima satisfeita pelo usuário em 2026-11-16.** A evidência disponível inclui 22 testes NUnit EditMode verdes na captura do usuário e relatos de Play Mode cobrindo afundamento/game over, conclusão da campanha e configurações diferentes. Os logs enviados confirmam início da campanha, inicialização da HUD, spawn e a mecânica de um/dois cliques.

O Editor concluiu recompilação sem erros após as últimas alterações. Não foram executados pelo agente build, Unity Test Runner, Play Mode, smoke ou batchmode, conforme as regras locais. Permanecem para uma etapa posterior: build Android, teste em aparelho, revisão de legibilidade/localização da HUD em proporções diferentes e validação específica da diferença de versão Cinemachine.

## Ordem, dependências e critério de conclusão

Ordem executada: aprovação do plano → Etapas 0–5 → demonstração mínima aceita pelo usuário. O Android está ativo e o Build Profile `Android` foi criado. A demonstração valida o loop de gameplay para avançar o produto, mas não significa que o jogo esteja pronto para distribuição. O próximo objetivo é tornar a HUD legível e responsiva e, depois, substituir os visuais temporários por uma direção de arte consistente.

A fatia de código estará pronta para validação quando os contratos puros implementarem todas as regras do GDD, a cena usar apenas as autoridades previstas e os dados inválidos impedirem iniciar a campanha. O MVP só terá viagem executável quando o usuário aprovar a taxa de afundamento, os cronogramas, posições e raios de toque e a compatibilidade do pacote for confirmada no Unity.

## Auto-revisão do plano

- Cada resultado terminal, recompensa, retry, Continue, seleção por toque e pausa do GDD tem um dono e uma operação previstos no domínio.
- A separação entre Framework, gameplay puro e adaptadores de Unity está explícita; não há alteração planejada em package nem substituição de uma authority compartilhada.
- A dependência Experimental de câmera foi aceita; a diferença de versão Cinemachine e a resolução do projeto continuam como gate real, sem afirmar compatibilidade.
- A proposta de painel de resultado e a ausência de valores de balanceamento estão visíveis para a revisão do usuário.
- A cena primária como cena da campanha e Activity sem Content Profile seguem a composição consumer comprovada de Game Flow para uma única cena.
- Este documento distingue recompilação sem erros, testes apresentados pelo usuário e validação de gameplay relatada pelo usuário; build Android, aparelhos e compatibilidade de Cinemachine continuam pendentes.
