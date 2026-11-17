# UX Spec: Fluxo da Campanha Mobile

> **Status**: Baseline aprovada; detalhes de implementação pendentes
> **Author**: Usuário e UX designer
> **Last Updated**: 2026-11-17
> **Journey Phase(s)**: Menu, viagem, resultado e encerramento; sem mapa formal da jornada
> **Platform Target**: Android; paisagem fixa em todas as telas do MVP; Touch primário/completo; sem gamepad
> **Design References**: design/game-brief.md; design/gdd/voyage-system.md; design/art/art-bible.md, Seção 7
> **Template**: UX Spec

---

## Purpose & Player Need — Objetivo e necessidade do jogador

O fluxo permite iniciar uma campanha, entender o trajeto e a meta de relíquias, reagir aos macacos demoníacos, pausar, tentar novamente após naufrágio e escolher quando continuar ou ver o encerramento. Durante a navegação, o jogador precisa perceber o perigo e tocar nos alvos rapidamente sem perder o progresso.

## Player Context on Arrival — Contexto do jogador

O jogador começa num menu simples. A viagem é frenética e tem pressão de tempo; um tutorial pode pausar a ação para explicar uma mecânica do MVP. A chegada oferece uma breve comemoração. O naufrágio é uma derrota recuperável. A última relíquia exigida leva ao encerramento. Ainda não existe mapa formal da jornada do jogador.

## Navigation Position — Posição na navegação

- Menu principal → Viagem → Recompensa da ilha → Continue para a próxima viagem enquanto faltarem relíquias.
- Menu principal → Viagem → Recompensa da ilha → Ver encerramento → Cutscene final → Tela de conclusão.
- Viagem → Animação de naufrágio → Game Over → Retry repete o cronograma atual ou Voltar ao menu após confirmação abandona a campanha.
- Viagem → Pausa → Retomar, Configurações ou sair para o menu após confirmação.
- Configurações são acessíveis pelo menu principal e pela pausa; ao sair, retornam à tela que as abriu.
- Tutorial aparece durante a viagem e só retorna à tentativa após Continue explícito.

## Entry & Exit Points — Entradas e saídas

| Origem | Gatilho | Contexto carregado |
|---|---|---|
| Abertura do app | App inicia | Campanha ativa não é restaurada entre execuções. |
| Menu principal | Tocar Jogar | Nova sessão de campanha. |
| Viagem | Tocar pausa ou Voltar do Android | Tentativa e progresso preservados enquanto pausados. |
| Retorno do app | App volta do segundo plano | Tentativa pausada; exibir o menu de pausa. |
| Viagem | Tutorial bloqueante | Estado atual preservado; gameplay pausado até Continue. |
| Viagem | Navio alcança a ilha | Progresso de relíquias e viagem concluída. |
| Viagem | Navio afunda | Progresso de relíquias e tentativa que falhou. |

| Destino | Gatilho | Observações |
|---|---|---|
| Viagem | Jogar no menu | Inicia nova campanha. |
| Viagem | Retomar na pausa | Continua a tentativa preservada. |
| Menu principal | Confirmar Sair para menu na pausa | Abandona a campanha; confirmação obrigatória. |
| Menu principal | Confirmar Voltar ao menu em Game Over | Abandona a campanha; confirmação obrigatória. |
| Viagem seguinte | Continue após recompensa não final | Inicia o cronograma seguinte; não concede relíquia. |
| Cutscene final | Tocar Ver encerramento | Início explícito após última recompensa. |
| Retry | Tocar após Game Over | Repete cronograma e mantém relíquias da sessão. |
| Tela de conclusão | Cutscene termina | Jogar novamente e Voltar ao menu. |
| Configurações | Voltar ou fechar | Retorna à tela de origem: menu principal ou pausa. |

## Layout Specification — Especificação de layout

### ASCII Wireframe — Esboço ASCII

~~~text
┌──────────────────────────────────────────────────────────────┐
│ [Pausa]*    [barco]──────────────────────[baú de destino]    │
│             [relíquia 1] [relíquia 2] ... [relíquia N]       │
│                 fundo adapta à proporção da tela             │
│              navio inteiro, macacos e água visível           │
│                  [rosto] [diálogo ou tutorial]               │
│   área de canhão futura                 área de canhão futura│
│                   (ocultas no MVP)                           │
└──────────────────────────────────────────────────────────────┘
~~~

O botão de pausa fica no canto superior esquerdo, dentro da safe area e fora da linha do trajeto. O esquema representa a composição aprovada em paisagem; a orientação fica travada em todas as telas do MVP, sem definir ancoragem na Unity.

### Main Menu — Menu principal

A capa do menu usa uma ilustração panorâmica em paisagem que conta a perseguição em três planos: a bruxa aparece no alto à direita enviando os macacos demoníacos; os macacos avançam pelo plano intermediário; e o capitão, maior e mais próximo, foge em primeiro plano à esquerda. O título cartunesco “Macacos me mordam” ocupa o alto à esquerda. Os botões **Jogar** e **Configurações** ficam no canto inferior direito, dentro da safe area, sobre uma área visualmente simples. **Jogar** usa o âmbar do tesouro como destaque; **Configurações** usa creme e azul-tinta. Título e controles não devem cobrir os rostos nem interromper a leitura da perseguição.

### Settings — Configurações

A tela reutiliza a ilustração da capa com contraste reduzido, atrás de um painel creme amplo e centralizado. O painel tem duas colunas:

| Coluna | Conteúdo |
|---|---|
| Esquerda | Idioma: English (US) e Português (Brasil), com a opção ativa indicada por cor e por contorno ou símbolo. |
| Direita | Música e Efeitos, cada um com slider de 0–100% e o valor percentual visível; 0% é mudo e 100% é o volume máximo. |

O botão **Voltar** fica na base do painel, dentro da safe area, e retorna à tela de origem: menu principal ou pausa. Títulos usam tipografia cartunesca; rótulos e controles usam tipografia limpa. Selecionar um idioma atualiza imediatamente os textos e grava a escolha. Ajustar os sliders aplica o volume e salva automaticamente, sem botão separado. As alterações persistidas devem passar por IPlayerPreferencesStore, sem acesso direto da interface a PlayerPrefs.

### Pause — Pausa

Ao pausar, a cena da viagem permanece congelada e visível sob uma camada escura. Um painel creme compacto e centralizado, dentro da safe area, mostra o título **Pausado** e as ações em ordem vertical: **Retomar** com destaque âmbar, **Configurações** como ação secundária e **Sair para menu** separado com tratamento de alerta. Escolher **Sair para menu** abre uma confirmação que explica que a campanha atual será abandonada; cancelar mantém a pausa e confirmar retorna ao menu principal. Configurações retorna à pausa ao fechar. Com a pausa aberta, Voltar do Android retoma a mesma viagem e não abandona a campanha. O Framework continua responsável pelo estado/Gate de pausa e pela superfície de apresentação.

#### Exit Confirmation — Confirmação de saída

Ao selecionar **Sair para menu**, a cena pausada e o painel de pausa permanecem ao fundo, escurecidos. Um cartão creme central pergunta **Sair para o menu?** e informa: “A campanha atual será abandonada. Para jogar novamente, será preciso iniciar uma nova campanha.” O texto deve ser localizado para en-US e pt-BR; esta redação em português é a cópia aprovada para o rascunho.

Os botões ficam lado a lado. **Ficar na pausa**, à esquerda e em destaque âmbar, fecha apenas a confirmação e preserva a tentativa pausada. **Sair para menu**, à direita e com tratamento visual de alerta, confirma o abandono e retorna ao menu principal. Voltar do Android com a confirmação aberta cancela a confirmação e retorna à pausa; a tentativa continua pausada.

### Arrival & Reward — Chegada e recompensa

Quando a viagem chega à ilha, a cena de navegação continua visível e escurecida atrás do pop-up. A manchete pt-BR **Relíquia encontrada!** aparece acima da ilha; a versão en-US será localizada. O pop-up surge com uma ilha pequena, com palmeira, grama e arbustos, e o baú ainda fechado; o capitão não aparece. Em seguida, o baú se abre e a relíquia sai dele com efeitos de comemoração. Toda a sequência acontece dentro do mesmo pop-up.

Abaixo da ilha, do baú e da relíquia, o progresso mostra quantas relíquias foram conquistadas e quantas ainda faltam. A base do pop-up fica reservada para a ação seguinte, dentro da safe area. Nenhuma ação aparece antes de a revelação terminar. Tocar durante a animação acelera a sequência, sem dispensar o resultado. Após a revelação, **Continue** inicia a próxima viagem enquanto faltarem relíquias; ao completar o total da campanha, **Ver encerramento** substitui Continue. A paleta âmbar e creme destaca o tesouro sobre o fundo escurecido.

### Game Over — Fim de jogo

Após a animação do naufrágio, a cena permanece visível e escurecida atrás de um painel creme central. O painel apresenta **Game Over**, o progresso das relíquias no formato obtidas/necessárias — por exemplo, **2/3** — e duas ações. **Retry** é a ação principal, em âmbar; reinicia o cronograma da viagem que falhou e mantém as relíquias conquistadas. **Voltar ao menu** é a ação secundária com tratamento de alerta; ao tocar, abre uma confirmação. Confirmar abandona a campanha e retorna ao menu; cancelar fecha a confirmação e mantém Game Over. A presença e a pose do capitão no fundo do naufrágio ainda não foram definidas.

### Completion — Tela de conclusão

Depois da cutscene final, uma ilustração panorâmica separada da cutscene celebra a libertação. O capitão, o navio e as relíquias reunidas na campanha ocupam o centro e a direita; uma área visualmente tranquila à esquerda reserva espaço para o título e as ações. A luz dourada se abre e os sinais violetas da maldição perdem espaço. A manchete pt-BR **Maldição quebrada!** aparece no alto à esquerda em tipografia cartunesca; a versão en-US será localizada. Na parte inferior esquerda, dentro da safe area, **Jogar novamente** é a ação principal em âmbar e inicia uma nova campanha; **Voltar ao menu** é a ação secundária.

### Dialogue & Tutorials — Diálogos e tutoriais

O painel horizontal fica centralizado na faixa inferior do mar, dentro da safe area. O retrato de quem fala aparece à esquerda; o texto localizado fica à direita, em tipografia limpa e sobre fundo de alto contraste. A caixa não cobre o casco, os macacos ou a linha d’água, e mantém água visível para comunicar o afundamento. Os cantos inferiores continuam reservados para canhões futuros, que não aparecem no MVP.

Mensagens de contexto usam essa composição sem pausar a viagem e desaparecem após uma duração que acompanha o tamanho do texto dentro de limites configuráveis. Tutoriais bloqueantes usam a mesma caixa, pausam a viagem pelo Framework e mostram **Continue** explícito no canto inferior direito. O jogador precisa tocar Continue para fechar o tutorial e retomar; nenhum toque fora do botão o dispensa.

### Layout Zones — Zonas de layout

1. **HUD superior:** botão de pausa no canto superior esquerdo da safe area, fora da linha horizontal do trajeto. O barco avança conforme o progresso ativo; ícone de baú marca o destino. Abaixo, exatamente um marcador por relíquia exigida.
2. **Área de jogo:** navio inteiro e alvos tocáveis visíveis. Preservar navio e zonas da HUD em proporções diferentes, adaptando o fundo.
3. **Água e mensagens:** mar permanece visível abaixo do navio. Diálogo/tutorial centralizado nessa faixa, rosto à esquerda e texto à direita; não cobrir navio nem macacos.
4. **Controles futuros:** reservar os cantos inferiores para dois canhões futuros; nenhum controle de canhão aparece no MVP.
5. **Sobreposições:** chegada e Game Over escurecem a cena atrás de um pop-up. O capitão não aparece no pop-up da ilha.

**Regra de safe area:** HUD, textos e controles interativos ficam dentro da safe area do Android; navio, mar e fundo podem ocupar as bordas da tela.

**Referência de composição:** 2340×1080 em paisagem (19,5:9). É uma base de referência para compor o layout, não uma exigência da resolução física do aparelho. A proporção aproxima os viewports mobile mais reportados no Brasil em outubro de 2026: 414×896 (11,33%) e 384×832 (10,87%). Esses dados de navegação mobile não representam uma média universal dos pixels físicos dos painéis ([StatCounter Brasil](https://gs.statcounter.com/screen-resolution-stats/mobile/brazil); [diferença entre viewport e pixels físicos no Android](https://developer.android.com/develop/ui/views/layout/webapps/targeting)).

### Component Inventory — Componentes

| Componente | Conteúdo | Interativo |
|---|---|---|
| Linha do trajeto | Progresso do barco e baú de destino | Não |
| Marcadores de relíquia | Um por relíquia exigida; estado obtida/não obtida | Não |
| Pausa | Abre o menu de pausa | Sim |
| Navio e macacos | Gameplay e afundamento | Macacos recebem toque |
| Painel de diálogo/tutorial | Retrato de quem fala à esquerda, texto localizado à direita, na faixa inferior do mar | Continue em tutorial bloqueante |
| Pop-up de chegada | Ilha, baú fechado, relíquia, progresso e ação seguinte | Toque acelera; Continue ou Ver encerramento |
| Game Over | Mensagem, progresso de relíquias, Retry e Voltar ao menu após confirmação | Sim |
| Tela de conclusão | Ilustração de celebração, manchete e ações Jogar novamente/Voltar ao menu | Sim |
| Configurações | Idioma e volumes de música/efeitos | Sim |
| Áreas futuras de canhão | Cantos inferiores reservados | Ausentes no MVP |

### Information Hierarchy — Hierarquia de informação

1. Localizar macacos e perceber o perigo pelo navio e pelas reações do capitão.
2. Acompanhar o trajeto e a meta de relíquias.
3. Ler tutorial ou mensagem de contexto sem perder o navio.
4. Alcançar pausa e configurações.

O MVP não tem medidor separado de afundamento. Navio e reações do capitão comunicam o perigo.

## States & Variants — Estados e variantes

| Estado | Gatilho | O que muda |
|---|---|---|
| Carregamento | Cena/campanha carrega | Apresentação e duração não definidas; não exibir a navegação como pronta antes da inicialização. |
| Campanha nova | Sessão inicia com zero relíquias | Progresso 0/N e marcadores não obtidos. |
| Menu principal | App abre ou jogador volta | Título, Jogar e Configurações. |
| Navegação | Campanha inicia/retoma | Linha avança; macacos surgem e recebem toques. |
| Mensagem de contexto | Dica sem pausa | Aparece e some; duração acompanha texto dentro de limites configurados. |
| Tutorial bloqueante | Instrução precisa de atenção | Viagem e gameplay pausam até Continue. |
| Pausa | Ícone, Voltar do Android ou retorno do segundo plano | Retomar, Configurações, Sair para menu. |
| Recompensa da ilha | Viagem chega | Cena escurecida; ilha e baú centrais; baú fechado abre e revela relíquia. |
| Game Over | Naufrágio termina | Cena escurecida atrás de painel central com Game Over, progresso das relíquias, Retry e Voltar ao menu após confirmação. |
| Encerramento | Última relíquia revelada e Ver encerramento tocado | Cutscene seguida da tela de conclusão. |
| Conclusão | Cutscene termina | Ilustração separada, manchete Maldição quebrada!, Jogar novamente e Voltar ao menu. |
| Dados inválidos | Configuração impede a inicialização | HUD de navegação não aparece como pronta; mostrar aviso localizado com Voltar ao menu. |

## Interaction Map — Mapa de interação

| Ação | Entrada | Feedback | Resultado |
|---|---|---|---|
| Atingir macaco | Toque na área do alvo | Reação e mudança de expressão por acerto | Cada toque atinge no máximo um macaco; toques simultâneos em macacos diferentes são aceitos; sobreposição escolhe o centro mais próximo de cada ponto tocado. |
| Pausar | Toque em pausa ou Voltar do Android | Solicita Pause ao Framework e abre sua superfície | O jogo bloqueia entrada e interrompe avanço da viagem, surgimentos e afundamento enquanto o estado/Gate do Framework estiver pausado. |
| Retomar | Tocar Retomar ou pressionar Voltar do Android com a pausa aberta | Fecha pausa | Mesma tentativa continua. |
| Cancelar saída | Tocar Ficar na pausa | Fecha confirmação | A pausa e a tentativa permanecem abertas. |
| Voltar na confirmação | Pressionar Voltar do Android com a confirmação aberta | Fecha confirmação | Retorna à pausa; a tentativa continua pausada. |
| Confirmar saída | Tocar Sair para menu na confirmação | Fecha confirmação e pausa | Abandona a campanha e retorna ao menu principal. |
| Configurações | Tocar no menu ou pausa | Abre configurações | Altera idioma e volumes; ao fechar, retorna à tela de origem. |
| Sair para menu | Tocar e confirmar | Abre confirmação | Confirmar abandona campanha; cancelar mantém pausa. |
| Continuar tutorial | Tocar Continue explícito | Fecha tutorial | Viagem retoma; toque acidental não dispensa instrução. |
| Ler contexto | Sem ação obrigatória | Mensagem aparece e some | Viagem continua. |
| Acelerar recompensa | Tocar durante animação | Revelação acelera | Ação seguinte só aparece após revelar a relíquia. |
| Continue | Tocar após recompensa não final | Fecha pop-up | Inicia cronograma seguinte. |
| Ver encerramento | Tocar após última revelação | Inicia cutscene | Exibe final da campanha. |
| Retry | Tocar após Game Over | Reinicia tentativa | Repete cronograma e mantém relíquias da sessão. |
| Voltar ao menu após Game Over | Tocar Voltar ao menu e confirmar | Abre confirmação | Confirmar abandona a campanha e retorna ao menu; cancelar mantém Game Over. |
| Enviar app ao segundo plano | Ação do sistema | Simulação pausa | Ao retornar, exibe pausa sem avanço retroativo. |
| Voltar ao menu após erro | Tocar Voltar ao menu no aviso localizado | Fecha o aviso | Retorna ao menu principal; a campanha não inicia. |

Agarrar/arrastar macacos, arremessar e carregar/disparar canhões estão fora das interações do MVP.

## Data Requirements — Requisitos de dados

| Dado | Proprietário | Leitura/escrita | Atualização e observações |
|---|---|---|---|
| Duração e cronograma | Configuração da campanha | Leitura | 60–300 segundos; configuração atual usa 180 segundos. |
| Progresso e tempo ativo | Simulação da viagem | Leitura | Atualiza durante tempo ativo e para durante pausa. |
| Classe, toques restantes, posição e estado dos macacos | Simulação e dados da campanha | Leitura/intenção de toque | Atualiza em surgimento e toque; seleção pelo centro mais próximo. |
| Afundamento e reação do capitão | Simulação e apresentação | Leitura | Atualiza durante avanço ativo; são os únicos sinais de afundamento no MVP. |
| Relíquias obtidas e exigidas | Activity/configuração da campanha | Leitura | Atualiza na chegada; controla marcadores, progresso e ação final. |
| Textos localizados | Dados de localização | Leitura | en-US (padrão) e pt-BR (alternativa); atualiza quando o idioma muda. |
| Idioma e volumes | Configurações do jogo via IPlayerPreferencesStore | Leitura/escrita | UnityPlayerPreferencesStore persiste idioma, música e efeitos em PlayerPrefs como backend local intermediário. Padrão: en-US e volumes 1.0; sliders de 0–100% gravam valores normalizados de 0–1. |

## Framework Boundary — Limites de responsabilidade

A aplicação usa com.immersive.framework como autoridade para ciclo de vida e fluxo: Routes cuidam da navegação entre menu, campanha e conclusão; Activity cuida da entrada, saída e reinício do contexto; Pause fornece requests, estado/Gate e superfície de apresentação. Não criar um segundo gerenciador de rotas, ciclo de vida, pausa ou reinício dentro do jogo. O jogo define o conteúdo e os resultados desses estados: campanha, viagem atual, cronograma, relíquias, condições de chegada/naufrágio e a semântica do Retry.

No pacote local com.immersive.framework 1.2.0 referenciado pelo projeto, Route/Activity e a superfície single-player de Pause constam como Stable; ActivityRestartTrigger e Reset são Experimentais. A sessão da campanha permanece no conteúdo da Route e o Resettable do controller usa membership Activity com alvo CurrentActivity. O botão Retry solicita Activity Restart; a regra CampaignSession.TryRetry é executada no callback de Reset, e avanço/toque ficam bloqueados até o resultado terminal. Sucesso libera o jogo; falha de Reset, Clear ou Reenter permanece bloqueada e diagnosticada. A superfície de Pause apresenta o estado, enquanto o jogo consulta o snapshot do Framework para bloquear entrada e interromper avanço do tempo, surgimentos e afundamento. A ligação de componentes no Editor e a validação do ciclo completo Reset → Clear → Reenter ainda estão pendentes.

Persistent Content é um escopo de cenas na vida da aplicação: os objetos permanecem durante trocas de Route/Activity, mas isso não grava preferências para sobreviver ao encerramento do app. Progression Save oferece contrato de slots Save/Load/Delete para progresso persistente do jogo. O contrato do store é Stable, mas runtime, perfil e backend JSON são Experimental. ADR-018 separa explicitamente Progression Save de Preferences; não usar slots de progresso para armazenar idioma ou áudio.

A decisão de persistir idioma e volumes de música/efeitos está aprovada. A semântica fica nas Configurações do jogo. Neste MVP, IPlayerPreferencesStore no assembly puro do jogo define o contrato; UnityPlayerPreferencesStore usa Unity PlayerPrefs como backend local intermediário, sem criar uma authority de ciclo de vida nem copiar código do Framework. Quando o Framework voltar a oferecer Preferences, trocar o backend atrás desse contrato. A composição visual de Configurações está definida neste documento; sua implementação ainda deverá carregar e salvar através de IPlayerPreferencesStore, sem acessar PlayerPrefs diretamente.

O jogo é dono do estado; a interface não calcula peso, afundamento, cronograma ou recompensa. O progresso da campanha é apenas da sessão, conforme o brief.

## Events Fired — Eventos emitidos

Esta baseline não define eventos de analítica nem nomes/payloads novos de log para as ações do mapa de interação. As ações seguem os contratos existentes de jogo e aplicação. O contrato de diagnóstico já usado pelo projeto não é alterado aqui.

## Transitions & Animations — Transições e animações

- Mensagens de contexto não pausam; a duração acompanha o tamanho do texto e respeita limites mínimo/máximo ainda sem valores.
- Tutoriais bloqueantes pausam até Continue.
- Chegada: pop-up sobre cena escurecida; baú começa fechado, abre e revela relíquia. Toque acelera, mas a ação seguinte espera a revelação.
- Quando o casco submerge por completo, ocorre naufrágio e a tripulação se joga ao mar; depois aparece Game Over.
- O capitão permanece no navio e demonstra pânico crescente; não aparece no pop-up da ilha.
- Menu → Campanha: fade escuro e indicador simples de carregamento na superfície persistente do Framework; o MVP não precisa de tela ilustrada temática.
- Transições de pausa e Configurações, duração e easing exatos, e sons ainda não foram definidos.

## Input Method Completeness Checklist — Checklist de entrada

**Entradas:** toque primário/completo; sem gamepad; Voltar do Android abre Pausa durante a viagem.

- [x] Área de toque um pouco maior que a silhueta, configurável e sem exagero.
- [x] Alvos sobrepostos selecionam o centro mais próximo.
- [x] Paisagem prioriza alcance com duas mãos e dois polegares.
- [x] Toques simultâneos em macacos diferentes são aceitos.
- [x] Tutorial exige Continue explícito.
- [x] Voltar do Android durante viagem abre pausa, sem abandonar campanha.
- [x] HUD, textos e controles respeitam a safe area; a arte pode chegar às bordas.
- [ ] Limites numéricos das áreas de toque precisam de validação em aparelho.
- [ ] Foco e APIs de acessibilidade Android não estão especificados.

## Accessibility — Acessibilidade

- Diferenciar classes, perigo e recompensa por forma, contorno, ícone ou expressão além da cor.
- Manter áreas de toque ajustáveis e um pouco maiores que as silhuetas.
- Mostrar reação clara a cada toque; macacos de múltiplos toques mudam expressão.
- Tipografia limpa na HUD, em diálogos e nos controles; cartunesca nos títulos.
- Manter texto em painel/fundo legível, não sobre cenário detalhado.
- Sem nível formal, tamanho mínimo, contraste, leitor de tela ou opção de reduzir movimento definidos.

## Localization Considerations — Considerações de localização

- Idiomas do MVP: en-US (padrão) e pt-BR (alternativa).
- Em uma instalação nova, iniciar em en-US, independente do idioma do aparelho. O jogador pode selecionar pt-BR em Configurações.
- Localizar textos de menu, pausa, tutorial, resultado, recompensa e erro de inicialização. O jogador relatou textos ausentes durante o jogo; implementação deve conferir todas as strings visíveis.
- Duração das mensagens considera comprimento localizado e respeita limites configurados.
- Testar expansão de texto de até 40% nas proporções suportadas. Limites máximos de caracteres e fallback de string ausente não foram definidos.
- Idioma escolhido e volumes de música/efeitos persistem entre execuções; os sliders mostram 0–100% e os valores são armazenados normalizados de 0–1.

## Acceptance Criteria — Critérios de aceitação

- [ ] Na referência 2340×1080 (19,5:9) e nos extremos de proporção definidos para Android, navio inteiro e zonas da HUD permanecem visíveis; o fundo adapta.
- [ ] Em todas as telas, HUD, textos e controles ficam dentro da safe area do Android; o navio, mar e fundo podem chegar às bordas.
- [ ] Em todas as telas do MVP, girar o aparelho não muda a orientação do jogo para retrato.
- [ ] O marcador chega ao baú após a duração ativa configurada; pausa não avança o tempo.
- [ ] Há exatamente um marcador por relíquia exigida e o estado obtido não depende apenas da cor.
- [ ] Toques simultâneos em macacos diferentes acertam ambos; cada toque atinge no máximo um macaco e sobreposições escolhem o centro mais próximo.
- [ ] O painel de diálogo/tutorial fica na faixa do mar com retrato de quem fala à esquerda e texto à direita, sem cobrir casco ou macacos; contexto não pausa e tutorial bloqueia até tocar Continue.
- [ ] Pausa e retorno do segundo plano preservam a tentativa; Voltar do Android abre Pausa durante a viagem e, com Pausa aberta, retoma a mesma tentativa.
- [ ] A confirmação de saída explica o abandono; Ficar na pausa e Voltar do Android cancelam sem retomar a viagem; Sair para menu só abandona após confirmação.
- [ ] Após a animação do naufrágio, Game Over mostra o fundo escurecido, progresso de relíquias obtidas/necessárias e as ações Retry e Voltar ao menu.
- [ ] Retry repete o cronograma da viagem que falhou e mantém as relíquias; Voltar ao menu exige confirmação, abandona a campanha e, ao cancelar, mantém Game Over aberto.
- [ ] Chegada mostra a navegação escurecida atrás do pop-up; a manchete “Relíquia encontrada!” fica acima da ilha, do baú e da relíquia no centro, sem o capitão. O baú abre e revela a relíquia com efeitos de comemoração.
- [ ] O progresso abaixo mostra relíquias conquistadas e quantas faltam. Tocar acelera a revelação; a ação na base só aparece depois dela: Continue para viagens restantes ou Ver encerramento após a última relíquia.
- [ ] Menu e estados do fluxo exibem textos en-US/pt-BR sem corte nem string ausente; uma instalação nova inicia em en-US.
- [ ] Após selecionar pt-BR em Configurações, a escolha continua ativa ao fechar e reabrir o app.
- [ ] Sliders de música e efeitos mostram 0–100%, convertem para valores normalizados de 0–1 e preservam esses valores após fechar e reabrir o app via IPlayerPreferencesStore.
- [ ] Alterar idioma ou volume aplica a mudança e a salva automaticamente, sem botão Aplicar/Salvar.
- [ ] Ao tocar Jogar, a troca para a campanha mostra fade escuro e indicador simples de carregamento na superfície persistente do Framework, sem tela ilustrada temática no MVP.
- [ ] Ao fechar Configurações, o jogador retorna ao menu principal ou à pausa que a abriu, sem abandonar a campanha.
- [ ] Campanha inválida não apresenta HUD como pronta; mostra aviso localizado e o botão Voltar ao menu retorna ao menu principal.
- [ ] A capa do menu mostra a bruxa acima da perseguição, os macacos no plano intermediário e o capitão em primeiro plano; título e botões respeitam a safe area sem cobrir rostos ou prejudicar a leitura da cena.
- [ ] Configurações apresenta idioma à esquerda e sliders de música/efeitos à direita, mostra percentuais de 0–100% e retorna ao menu ou à pausa que abriu a tela.
- [ ] Pausa mantém a viagem congelada sob a sobreposição escura; Configurações retorna à pausa e Sair para menu exige confirmação antes de abandonar a campanha.
- [ ] Após a cutscene final, a conclusão mostra a ilustração separada do capitão, navio e relíquias, a manchete “Maldição quebrada!” e as ações Jogar novamente/Voltar ao menu dentro da safe area.
- [ ] Em aparelho-alvo, feedback ao toque parece imediato e não espera atualização tardia do trajeto. Orçamento numérico fica para quando houver aparelhos-alvo.

## Open Questions — Questões em aberto

- Não há mapa da jornada; contexto vem do brief e das decisões aprovadas.
- A presença e a pose do capitão no fundo do pop-up de Game Over ainda não foram definidas.
- Pausa manual, de tutorial e de segundo plano devem usar o estado/Gate e a superfície de Pause do Framework; falta validar a composição no Unity sem criar um dono de pausa paralelo no jogo.
- ActivityRestartTrigger e Reset são Experimentais no Framework local 1.2.0; o consumidor já usa Retry via Activity Restart e Reset com membership Activity. A autoria dos componentes na cena e a validação manual de preservação e falhas continuam pendentes.
- Nenhum GDD de sistema possui seção própria de requisitos de UI; este inventário deriva das decisões aprovadas, brief e GDD de viagem.
- A superfície persistente de loading/fade do Framework ainda precisa ser composta nos assets do projeto e validada em Unity.
- As proporções de tela suportadas ainda precisam ser escolhidas e validadas em aparelho.
- Limites numéricos das áreas de toque estão em aberto e precisam de validação em aparelho.
- Limites mínimo/máximo de duração das mensagens não têm valores.
- Escala dos sliders (0–100%), persistência normalizada (0–1) e aplicação/salvamento automáticos estão definidos. A conexão dos sliders aos volumes de áudio ainda precisa de implementação e validação.
- O pacote Framework local 1.2.0 não fornece Preferences no código ativo; o backend PlayerPrefs do jogo é intermediário e deve migrar quando uma API pública de Preferences estiver disponível.
- A composição da tela de Configurações está registrada; a implementação ainda está pendente e deve consumir IPlayerPreferencesStore para carregar e salvar idioma e volumes.
- Fallback para string localizada ausente ainda não foi definido.
- Nível de acessibilidade e integração com APIs Android precisam de decisão.
- Aparelhos-alvo não definidos; manter orçamento de desempenho em aberto.
- Tutoriais de agarrar/arremessar e canhões, e os próprios controles de canhão, ficam fora do MVP.
