# Framework Application Boot Contract

**Status:** Auditoria estática concluída; APIs Experimentais de câmera aceitas para o MVP; validação Unity pendente.
**Data:** 2026-11-15
**Consumidor:** Macacos me Mordam, Unity 6000.6.9f1
**Fonte de design:** [Voyage System](../gdd/voyage-system.md) e [Campaign Activity design](../../docs/superpowers/specs/2026-11-15-campaign-activity-design.md)

## Escopo e conclusão

Este documento registra o caminho real de inicialização do Immersive Framework resolvido pelo projeto, as verificações de autoria disponíveis e a menor composição compatível com a campanha. A campanha usa uma Route e uma Activity; viagens, tentativas, macacos, afundamento e relíquias continuam sob propriedade do jogo.

O projeto ainda não tem Game Application, Route ou Activity. O asset de Settings aponta para Active Game Application nula. Pela cadeia de código examinada, a inicialização atual para no primeiro gate de aplicação e registra falha por ausência de Game Application. Essa conclusão é estática; não foi observada no Unity Editor.

## Identidade e resolução do consumidor

| Evidência | Estado observado |
|---|---|
| Raiz Unity | Este repositório contém a raiz do projeto Unity dentro de Games/macacos-a-bordo/Macacos Me Mordam. |
| Unity | ProjectVersion.txt declara 6000.6.9f1, revisão 8c45277fb9c5. |
| Git do jogo | Branch main, HEAD c2606b1d28613ee8f660140c6ee70b966a601c93. manifest.json e packages-lock.json já tinham alterações antes desta auditoria; foram preservados. |
| Manifest e lock | com.immersive.framework é uma dependência local file:, fonte local no lock. O manifesto usa um caminho absoluto específico da máquina; a alteração permanece local e não deve ser publicada como configuração portátil. |
| Framework local | package.json identifica com.immersive.framework, versão 1.2.0, Unity mínimo 6000.5. Declara Composition 0.1.0-preview.3, Foundation 0.2.3, Logging 0.2.4, Cinemachine 3.1.7 e Input System 1.19.0. O lock resolve Composition 0.1.0-preview.3, Foundation 0.2.3, Logging 0.2.4, Input System 1.20.1 direto pelo projeto e Cinemachine 6.6.0 como pacote builtin. A resolução de Cinemachine diverge da versão declarada pelo Framework; compatibilidade de importação/compilação ainda não foi confirmada no Unity. |
| Maturidade publicada | O README público do repositório apresenta 1.1.0 como primeiro baseline Stable e distingue versão do pacote de maturidade por API. O conteúdo local declara 1.2.0, mas a revisão Git e a identidade de release não puderam ser verificadas: o acesso de leitura a .git da cópia local foi negado pelo ambiente. Assim, as maturidades abaixo descrevem a documentação e os atributos da cópia local, sem afirmar que 1.2.0 é um release publicado. |
| README do jogo | Ainda descreve Framework 1.1.0 via OpenUPM, divergindo do manifest/lock local atual. O README precisa de atualização documental antes de servir como instrução de setup compartilhada. |

O editor de ProjectVersion confirma Unity 6000.6.9f1, acima do mínimo 6000.5 declarado pelo package.json local e pelo README público. A referência oficial disponível é o [README do Immersive Framework](https://github.com/ImmersiveGames/com.immersive.framework#readme).

## Estado atual dos assets

| Asset | Estado observado |
|---|---|
| ImmersiveFrameworkSettings | [Asset de Resources](../../Assets/_Project/Settings/ImmersiveFramework/Resources/ImmersiveFrameworkSettings.asset) existe. Active Game Application está nula; Editor Play Mode Startup está em 0, que o enum local define como FrameworkStartup; frameRatePolicy.mode está em 0, UseUnityDefaults. |
| Game Application, Route e Activity | Nenhum asset desses tipos aparece na pasta Assets do projeto. |
| Cena de exemplo | [SampleScene](../../Assets/Scenes/SampleScene.unity) contém um objeto Main Camera. A cena é o starter do Unity, ainda não uma composição aprovada da câmera do jogo. |

## Sequência de runtime observada

~~~text
AfterSceneLoad
  -> carrega ImmersiveFrameworkSettings de Resources
  -> no Editor, pode pular o boot somente com CurrentSceneOnly
  -> valida configuração inicial
  -> resolve Player Session opcional e cria o Runtime Host
  -> compõe Progression Save se habilitado
  -> aplica frame-rate policy
  -> carrega Primary Scene da Startup Route
  -> carrega Persistent Content e valida seu escopo
  -> cria Outputs da Camera Session e atribuições de Session
  -> compõe e confirma o escopo de Persistent Content
  -> inicia Game Flow: Route inicial e Startup Activity opcional
  -> conecta Local Player Provisioning se Player Session estiver habilitada
~~~

As classes de bootstrap e Runtime Host são internas. O projeto deve compor os assets documentados e observar os resultados; não deve chamar esses detalhes internos como API de gameplay.

## Gates de runtime

| Gate | Condição | Pré-requisito e falha | Fonte instalada e maturidade |
|---|---|---|---|
| Descoberta de Settings | Todo boot Framework | Resources deve conter ImmersiveFrameworkSettingsAsset no ResourcesPath. Sem asset, FrameworkBootValidator retorna falha. | ImmersiveFrameworkBootstrap.LoadSettings; FrameworkBootValidator.Validate. Internos. |
| Desvio de Play Mode | Apenas Unity Editor | CurrentSceneOnly pula a inicialização Framework no Editor. Player/runtime builds sempre iniciam o Framework. O asset atual usa 0 = FrameworkStartup. | ShouldSkipFrameworkStartupInEditor; FrameworkEditorPlayModeStartup é Stable na referência local. |
| Frame rate do projeto | Todo boot Framework | FrameRatePolicy deve existir e passar TryValidate. O modo padrão UseUnityDefaults é válido e mantém os valores do Unity. O asset atual serializa modo 0. | FrameworkBootValidator.Validate e FrameworkRuntimeHost.TryApplyApplicationFrameRatePolicy. ApplicationFrameRatePolicy é Experimental na cópia local. |
| Game Application ativa | Todo boot Framework | Settings.ActiveGameApplication deve apontar para GameApplicationAsset. Sem referência, a validação para com erro. É a falha prevista no estado atual do projeto. | FrameworkBootValidator.Validate. GameApplicationAsset e ImmersiveFrameworkSettingsAsset são Stable na documentação local. |
| Progression Save | Somente quando ProgressionSaveEnabled é true | Exige perfil padrão válido; a composição do backend também precisa ter sucesso durante a criação do Host. Desabilitado é ausência válida; o padrão do asset é false. | FrameworkBootValidator.Validate e ProgressionSaveApplicationComposition.Resolve. Perfil/backend não são necessários ao MVP sem save persistente aprovado. |
| Camera Session | Sempre; não há flag para desligar | CameraSession deve existir e TryValidate precisa passar. A configuração vazia padrão falha: é exigido ao menos um Output prefab válido. Cada prefab precisa de exatamente um CameraOutputAuthoring, CameraOutputDefinition válido, Unity Camera, CinemachineBrain e Fallback Camera Rig explícitos. Na materialização, a câmera Cinemachine do Fallback Rig também precisa existir; se não existir, o isolamento de canais falha e o Host não inicia. | FrameworkBootValidator.Validate, CameraSessionConfiguration.TryValidate, FrameworkRuntimeHost.StartCoreAsync e CameraSessionOutputMaterializationRuntime.TryCreate. CameraSessionConfiguration e CameraOutputDefinition são Experimental na cópia local. |
| Startup Route | Todo boot Framework | GameApplication.StartupRoute deve existir. Depois da validação inicial, Game Flow solicita essa Route; falha do ciclo da Route encerra a inicialização do Game Flow. | FrameworkBootValidator.Validate e GameFlowRuntime.StartAsync. RouteAsset é Stable na documentação local. |
| Primary Scene | Toda Startup Route | A Route precisa declarar Primary Scene. O Runtime Host tenta carregá-la antes de Persistent Content; falha de carregamento encerra o start. O Model Readiness também verifica caminho e inclusão no Build Profile. | RouteAsset.HasPrimaryScene; PrepareStartupPrimarySceneBeforeGlobalUiAsync; FrameworkAuthoringModelReadinessValidator.ValidateRouteModel. |
| Persistent Content | Todo start do Runtime Host | Depois de carregar a cena da Route, GlobalUiSceneRuntime exige Persistent Content com cena referenciada e carregável. Ausência ou caminho indisponível falha antes de Game Flow. A cena deve estar habilitada no Build Profile. O boot validator inicial não verifica esse gate. | FrameworkRuntimeHost.StartCoreAsync; GlobalUiSceneRuntime.LoadAndPersistAsync; ValidatePersistentContentComposition. Composition/template é Stable workflow na documentação local. |
| Startup Activity | Opcional para Framework; escolhida pelo jogo para a campanha | Quando presente em Route.StartupActivity, Game Flow inicia a Activity. Com política de readiness diferente de ObserveOnly, a configuração de readiness é validada e pode falhar; depois, uma Activity pode permanecer NotReady mesmo com navegação confirmada. Os defaults locais são NoSlots/Allowed e ObserveOnly. A navegação Route/Activity não exige Player admitido. | GameFlowRuntime.StartAsync; ActivityAsset; guias Getting Started e Game Flow. Assets e requests são Stable; readiness avançada é Experimental na referência local. |
| Player Session e provisioning | Somente quando PlayerSessionEnabled é true | Perfil inválido ou falha de inicialização aborta a criação do Host; provisioning é conectado após Game Flow quando configurado. Desabilitado é o padrão e a navegação de campanha não precisa de Player. | ImmersiveFrameworkBootstrap, FrameworkRuntimeHost.TryCreate e GameApplicationAsset. Não ativar Player Session apenas para receber toque de gameplay. |

### Dependências transitivas de câmera

Além de validar a Camera Session, o runtime exige que cada prefab de Output contenha exatamente um CameraOutputAuthoring. Esse componente liga um CameraOutputDefinition ao Unity Camera, CinemachineBrain e Fallback Camera Rig. O CameraOutputDefinition é Experimental, embora CameraOutputAuthoring seja Stable.

O CameraRigComposer também precisa de um Behavior Definition. Para a câmera fixa prevista no MVP, isso exige FixedCameraRigBehaviorDefinition, que é Experimental. A materialização runtime rejeita o Fallback Camera Rig se a referência CinemachineCamera ainda estiver nula. No Editor, Apply / Rebuild cria/reconcilia essa câmera e registra a referência no prefab; a ação só fica disponível em Edit Mode e altera o prefab. Salvar o prefab depois da ação é necessário para preservar a composição. CameraRigComposer é Stable; Apply / Rebuild é Development Tooling.

Assim, o caminho mínimo da câmera combina contratos Stable e Experimentais. O usuário aceitou essa dependência Experimental para o MVP. A configuração e materialização dos assets podem seguir após a revisão do plano de implementação; isso não confirma compatibilidade de importação ou compilação no Unity.

## Gates de autoria e alcance dos validadores

| Ação explícita do Editor | Caminho chamado | O que cobre e limite |
|---|---|---|
| Project Settings > Validate Configuration | FrameworkBootValidator.Validate | Verifica Settings, frame-rate, Game Application, Progression Save condicional, Camera Session, Startup Route e existência de Primary Scene. Não verifica o carregamento real da cena nem o gate de Persistent Content que ocorre depois. |
| Game Application Inspector > Validate | FrameworkAuthoringValidator.ValidateGameApplication, PlayerParticipationAuthoringValidator e ProgressionSaveAuthoringValidator | Verifica dependências de autoria da aplicação, Route, câmera, Persistent Content e identidade; valida também os domínios opcionais habilitados. É diagnóstico de autoria, não execução de runtime. |
| Project Settings > Advanced/Diagnostics > Run Model Readiness Check | FrameworkAuthoringModelReadinessAggregator.ValidateProjectReadiness(settings, true) | Agrega validação de Project Settings, Game Application, Startup Route, Activity presente, caminhos de cena/Build Profile, identidade e participação do Player. A chamada inclui bindings das cenas abertas. Não inicia cenas, não materializa Outputs e não garante que o Runtime Host concluirá o boot. |

As ações executam validadores e produzem relatórios; elas não criam assets nem reparam a configuração automaticamente. O botão Validate do Inspector de Game Application aplica primeiro as edições serializadas que o usuário já fez no Inspector. A ação separada Apply / Rebuild do CameraRigComposer altera a composição do prefab e requer salvamento explícito. Nenhum validador ou ação de materialização foi executado durante esta auditoria.

## Composição mínima proposta para o MVP

1. Atribuir um GameApplicationAsset em Project Settings > Immersive Framework.
2. Criar uma RouteAsset com Primary Scene e apontar Startup Activity para uma única ActivityAsset da campanha. O loop de viagem e seus retries continuam dentro do estado do jogo, sem criar uma Activity por tentativa.
3. Atribuir uma cena de Persistent Content por caminho de projeto e habilitá-la no Build Profile. O guia instalado recomenda criar a cena a partir do template e usar sua ação explícita de validação.
4. Configurar um Output de Camera Session e seu Fallback Camera Rig válidos, ainda que a campanha não use Player. Isso exige CameraOutputDefinition e FixedCameraRigBehaviorDefinition Experimentais, além de Camera Session Experimental. O prefab deve conter exatamente um CameraOutputAuthoring com Unity Camera, CinemachineBrain e Fallback Camera Rig referenciados. Em Edit Mode, Apply / Rebuild materializa a CinemachineCamera no prefab; salvar o prefab depois. A SampleScene contém Main Camera; a integração deve definir uma única autoridade física de câmera e evitar duas câmeras ativas.
5. Manter Player Session e Progression Save desabilitados no MVP. Usar o frame-rate padrão UseUnityDefaults até que aparelhos-alvo sejam selecionados.
6. Dar IDs válidos e únicos à Route e Activity. Os campos routeId e activityId começam vazios em novos assets e a validação de identidade inclui a cadeia Startup Route/Startup Activity.
7. Usar somente Route/Activity Content Profiles quando cenas adicionais forem parte real da composição. A campanha continua uma Activity; o código do jogo detém suas regras e recompensas.

### Decisão registrada sobre a autoria dos assets

O plano aprovado determina preferência por API Stable e só permite apoiar-se em API Experimental quando a auditoria comprova que é exigida e o usuário aceita o risco. A auditoria confirma que o bootstrap atual exige Camera Session, Output Definition e uma câmera Cinemachine materializada; a composição fixa mais simples também usa Fixed Camera Rig Behavior. O usuário aceitou essas superfícies Experimentais para o MVP. A criação de assets aguarda a revisão do plano de implementação e a compatibilidade do pacote com a versão de Cinemachine resolvida precisa ser confirmada no Editor.

Não criar bypass nem alterar o código do Framework para remover essa exigência. Uma mudança para tornar Camera Session opcional pertence ao Framework e exigiria avaliação no repositório proprietário do pacote; nenhuma alteração foi feita nesse pacote.

## Validação manual pendente

Unity não foi executada nesta etapa. Depois de compor os assets, executar no Editor 6000.6.9f1:

1. Abrir o projeto, aguardar resolução de pacotes/importação e confirmar que não há erros de compilação.
2. Atribuir o Game Application e verificar que Primary Scene e Persistent Content estão habilitados no Build Profile.
3. Executar Validate Configuration, Validate no Game Application Inspector e Run Model Readiness Check; resolver os erros sem depender de criação automática.
4. Entrar em Play Mode com FrameworkStartup. Observar log de boot, Route e Activity iniciais, Persistent Content e materialização da câmera.
5. Confirmar na Hierarchy/Game View que a câmera de saída pretendida renderiza e que a Main Camera starter não cria renderização duplicada.

Compilação/importação, Play Mode, build móvel e NUnit permanecem pendentes. O sistema operacional móvel e os aparelhos-alvo continuam sem decisão.

## Proveniência principal

- Game-owned design: [Voyage System](../gdd/voyage-system.md).
- Framework local resolvido: Documentation~/Guides/Getting-Started.md, Documentation~/Guides/Game-Flow.md, Documentation~/Guides/Persistent-Content-Scene-Template.md e Documentation~/API/Public-API.md.
- Runtime examinado: Runtime/Bootstrap/ImmersiveFrameworkBootstrap.cs, Runtime/Bootstrap/FrameworkBootValidator.cs, Runtime/ApplicationLifecycle/FrameworkRuntimeHost.cs, Runtime/ApplicationLifecycle/FrameworkRuntimeHost.FrameRate.cs, Runtime/GameFlow/GameFlowRuntime.cs e Runtime/GlobalUi/GlobalUiSceneRuntime.cs.
- Autoria/validação examinada: Runtime/Authoring/GameApplicationAsset.cs, Runtime/Authoring/ImmersiveFrameworkSettingsAsset.cs, Runtime/Authoring/RouteAsset.cs, Runtime/Authoring/ActivityAsset.cs, Runtime/CameraAuthoring/CameraSessionConfiguration.cs, Runtime/Camera/Bindings/CameraOutputAuthoring.cs, Runtime/CameraAuthoring/CameraRigComposer.cs, Runtime/CameraAuthoring/FixedCameraRigBehaviorDefinition.cs, Runtime/Camera/Lifecycle/CameraSessionOutputMaterializationRuntime.cs, Editor/CameraAuthoring/CameraRigComposerEditor.cs, Editor/CameraAuthoring/CameraRigComposerApplyRebuildUtility.cs, Editor/Validation/FrameworkAuthoringValidator.cs, Editor/Validation/FrameworkAuthoringModelReadinessValidator.cs, Editor/Validation/FrameworkAuthoringModelReadinessAggregator.cs e os respectivos Inspectors.
