# Voyage System

**Status:** Aprovado pelo usuário.
**Versão:** 0.4
**Fonte:** [Brief do jogo](../game-brief.md) e [Activity de campanha](../../docs/superpowers/specs/2026-11-15-campaign-activity-design.md)

## Intenção

Especificar uma tentativa de viagem do arcade e sua relação com o progresso para quebrar a maldição. Uma campanha permanece dentro de uma Route e Activity; cada viagem é um estado repetível do jogo, não uma nova Activity.

## Objetivos

- Fazer do percurso até a ilha o relógio da tentativa.
- Dar prioridade visual e mecânica aos macacos de maior peso.
- Permitir que o jogador reduza a pressão tocando nos macacos.
- Fazer a chegada conceder progresso e o afundamento encerrar a tentativa.
- Permitir novas tentativas sem perder relíquias já obtidas durante a sessão.

## Fora do escopo

- Salvamento persistente entre execuções do aplicativo.
- Balanceamento final da frequência e composição das ondas, da taxa de afundamento e dos raios de toque.
- Regiões, rotas, navios, chefes, melhorias ou economia adicionais.
- Definição da tela final, arte, animação e áudio.

## Proprietário e limites

O jogo é dono do estado da viagem, dos macacos, da carga do navio, do resultado e das relíquias. A sessão da campanha vive no conteúdo da Route para preservar progresso e tentativas durante o reinício da Activity. O Framework é dono da navegação por Route, do ciclo de Activity e do estado de Pause; o jogo define o resultado da regra de Retry e executa sua limpeza como participante de Reset. A entrada por toque envia uma intenção às regras do jogo; a cena e a HUD apresentam o estado resultante.

## Mecânicas principais

### Percurso e tempo

Cada viagem recebe uma duração em segundos configurável entre 60 e 300, inclusive (1 a 5 minutos). O Inspector usa 300 segundos ao criar um cronograma novo; a configuração atual mantém 180 segundos em cada viagem. A tentativa começa com progresso de percurso igual a zero. O progresso cresce apenas durante o tempo ativo segundo `progresso = tempo_ativo / duração_configurada`, limitado a 1. Ao alcançar 100%, o navio chega à ilha. A duração é um parâmetro de ajuste no Inspector.

### Surgimento dos macacos

Uma tentativa usa um cronograma ordenado de ondas, cada uma com progresso nominal entre zero e um. O dado de autoria configura a variação temporal de cada onda em segundos; o horário sorteado fica dentro desses limites. As janelas devem ficar depois da partida e antes da chegada, sem sobreposição ou reordenação. A ordem, a quantidade de macacos e a composição por classe ficam fixas conforme o cronograma; o sorteio não as altera.

Cada ocorrência de macaco tem um centro de surgimento em unidades locais do tabuleiro e semi-largura e semi-altura configuráveis, não negativas. A posição é sorteada uniformemente dentro desse retângulo. Isso permite definir áreas fixas ou limitadas e variar o padrão dentro delas.

Um único seed é usado quando a sessão da campanha constrói seus cronogramas. Seed `-1` sorteia um padrão para cada nova sessão; seed não negativo reproduz o mesmo padrão para ajuste. Os horários e posições sorteados ficam armazenados na definição usada pela sessão. Retry repete exatamente os mesmos horários e posições. Continue usa o cronograma da próxima viagem já sorteado. Uma nova sessão cria outro padrão quando o seed é `-1`.

Para cada onda, `rₑ = min(rₛ, p₀ × D / 2, (1 − p₀) × D / 2)` e `t = p₀ × D + U(-rₑ, rₑ)`, onde `p₀` é o progresso nominal, `D` é a duração configurada, `rₛ` é a variação máxima em segundos, `rₑ` é a variação efetiva e `U` é um sorteio uniforme; o progresso entregue à simulação é `p = t / D`. Essa margem mantém a onda depois da partida e antes da chegada mesmo em viagens curtas. Para cada ocorrência, `x = x₀ + U(-jₓ, jₓ)` e `y = y₀ + U(-jᵧ, jᵧ)`, usando o centro `(x₀, y₀)` e as semi-extensões `(jₓ, jᵧ)` definidos no Inspector. Exemplo: uma onda nominal em 5% com variação máxima de 3 s numa viagem de 60 s usa variação efetiva de 1,5 s e surge entre 1,5–4,5 s; uma onda em 15% pode variar entre 24–30 s numa viagem de 180 s ou 42–48 s numa viagem de 300 s.

### Classes e peso

O protótipo mecânico atual usa dois tipos. O MVP incluirá os Tipos 1–3; a versão final prevê cinco tipos. A tabela registra os valores atuais do protótipo para os Tipos 1–2 e o ponto de partida ajustável do Tipo 3 planejado para o MVP. Cada classe tem peso e `clicksToDefeat` configuráveis juntos no Inspector. Os valores atuais são um toque para a Leve (Tipo 1) e dois para a Pesada (Tipo 2). O Tipo 3, mais pesado, entra no MVP com três toques como ponto de partida ajustável; seu peso também é configurável e deve superar o do Tipo 2. Ao calibrar, classes mais pesadas devem exigir mais toques; a validação da campanha rejeita classes fora da ordem crescente de peso e toques.

| Classe | Peso inicial | Toques para derrotar |
|---|---:|---:|
| Leve (Tipo 1) | 1 | 1 |
| Pesado (Tipo 2) | 2 | 2 |
| Mais pesado (Tipo 3, MVP) | Configurável, maior que Tipo 2 | 3 (ponto de partida ajustável) |

Cada toque válido reduz em um o número de toques restantes definido para a classe. A ocorrência continua ativa e mantém todo o seu peso até receber o último toque; só então é removida da carga do navio. Na configuração atual, classe 1 requer um toque, classe 2 requer dois; uma nova classe recebe como ponto de partida o número da classe e pode ser calibrada no Inspector, mantendo a progressão crescente.

O peso total da viagem é a soma dos pesos dos macacos ainda ativos. Derrotar um macaco remove o peso dele das atualizações seguintes.

### Toque para derrotar

Um toque válido atinge no máximo um macaco ativo. Quando mais de um alvo contém o ponto tocado, vence o macaco cujo centro está mais perto do ponto. Distâncias iguais são resolvidas pela ordem de surgimento, do mais antigo para o mais novo. Um macaco só é derrotado quando seus toques restantes chegam a zero; até lá permanece ativo e mantém seu peso. A apresentação mostra os toques restantes junto ao peso.

O toque é avaliado contra os macacos já visíveis naquele instante; um macaco que surge no mesmo passo de atualização só pode ser tocado após surgir.

### Afundamento

O progresso de afundamento é acumulativo: derrotar macacos não recupera o nível já alcançado. A carga ativa determina a taxa futura de avanço. Sem macacos ativos, o progresso de afundamento fica parado.

Sejam:

- L: soma dos pesos dos macacos ativos;
- k: taxa de afundamento por unidade de peso e segundo, um parâmetro ajustável;
- Δt: tempo ativo desde a atualização anterior;
- S: progresso de afundamento entre zero e um.

A atualização é:

S_novo = min(1, S_anterior + L × k × Δt)

Ao chegar a um, o navio afunda. O valor de k não é balanceamento final.

### Pausa e retomada

A apresentação pode pausar a tentativa por ação manual, para um tutorial ou quando o aplicativo vai para segundo plano. Enquanto pausada, o percurso, o cronograma de surgimento, o afundamento e os toques de gameplay ficam suspensos. A tentativa preserva seu estado e não acumula tempo em pausa.

- Durante a navegação, o controle discreto de pausa ou o botão Voltar do Android abre o menu de pausa com Continuar, Configurações e Sair para menu. Voltar não abandona a campanha diretamente.
- Um tutorial que pausa a viagem só a retoma após a ação explícita Continue.
- Ao ir para segundo plano, a tentativa pausa automaticamente. Ao voltar, o mesmo menu de pausa é exibido; nenhum progresso retroativo é aplicado.
- Sair para o menu pede confirmação. Confirmar abandona a campanha atual; cancelar mantém a pausa aberta.

## Estados e resultados

### Tentativa de viagem

- **Sailing:** o percurso avança; macacos podem surgir e ser derrotados; o navio pode afundar.
- **Arrived:** o percurso chegou à ilha antes do afundamento.
- **Sunk:** o progresso de afundamento chegou a um antes da chegada.

Arrived e Sunk são resultados terminais. Depois do resultado, a tentativa não aceita toques nem novos surgimentos.

Se chegada e afundamento ocorrerem no mesmo instante, a chegada vence. Se o afundamento ocorrer primeiro, a tentativa falha, mesmo que a chegada ocorra no mesmo passo de atualização do motor.

### Progresso da campanha

O contador começa em zero e a campanha inicia no cronograma de índice zero. A configuração da campanha define `requiredRelics`, a quantidade positiva de relíquias necessária; o asset atual usa três. Cada chegada concede exatamente uma relíquia, uma única vez por tentativa. Quando o contador alcança `requiredRelics`, a campanha termina com a maldição quebrada e não inicia outra viagem.

Uma falha concede zero relíquias e deixa a campanha pronta para retry. Retry reinicia percurso, macacos, resultado e afundamento, preserva as relíquias da sessão e repete o mesmo índice de cronograma.

Uma chegada abaixo de `requiredRelics` avança o índice do cronograma exatamente uma vez e deixa a campanha em ReadyForContinue. Somente a ação explícita Continue inicia a próxima tentativa, com percurso e afundamento em zero e sem macacos. Continue não concede recompensa. Ao alcançar `requiredRelics`, a campanha termina com CurseBroken e não oferece Continue. A definição da campanha precisa conter ao menos `requiredRelics` cronogramas.

O progresso da campanha existe apenas na sessão atual do aplicativo e não é salvo entre execuções. Estados terminais da tentativa impedem uma segunda concessão da mesma recompensa.

## Dependências

- O cronograma de surgimento usa o progresso do percurso.
- A carga do navio usa somente os macacos ativos e seus pesos.
- Toques removem um único macaco ativo e atualizam a carga nas próximas operações.
- O resultado da viagem atualiza o contador de relíquias.
- A apresentação consome o estado e os eventos do jogo; ela não calcula peso, afundamento ou recompensa.

## Parâmetros de balanceamento

- Duração do percurso: 60–300 segundos por viagem; novos cronogramas começam em 300 segundos, e a configuração atual usa 180 segundos por viagem.
- Pontos, quantidade e composição dos grupos de surgimento.
- Peso das classes de macaco.
- Toques necessários por classe: campo `clicksToDefeat` no Inspector junto ao peso; a classe 1 começa em 1, a classe 2 em 2, e classes mais pesadas devem exigir mais toques.
- Taxa de afundamento por unidade de peso.
- Raio de toque de cada macaco.
- Variação temporal de cada onda, em segundos.
- Centro e semi-extensões retangulares de cada área de surgimento.
- Seed aleatório (`-1` para novo padrão por sessão; não negativo para repetição).
- Quantidade de relíquias necessária para quebrar a maldição: `requiredRelics` na configuração da campanha; o asset atual usa 3 e a definição precisa ter ao menos essa quantidade de cronogramas.

O baseline aprovado usa duração configurável de 60–300 segundos (valor de criação 300; configuração atual 180 por viagem), taxa de afundamento `k = 0,0015`, pesos atuais do protótipo 1 e 2 com um e dois toques, raios de toque 64 e 80 unidades locais, variação temporal inicial de ±3 segundos por onda e semi-extensões de área de 40 por 24 unidades locais. O Tipo 3 entra no MVP com três toques iniciais ajustáveis e peso maior que o Tipo 2. Duração, peso, `clicksToDefeat`, raios, `requiredRelics` e demais parâmetros ajustáveis permanecem na configuração da campanha ou no Inspector conforme seu proprietário; o número de toques deve aumentar junto com o peso. A faixa comparativa proposta para `k` é 0,0010–0,0020. O asset atual usa `requiredRelics = 3`; esse valor não é fixo para todas as campanhas.

## Cenários de aceitação

Os valores abaixo são somente um fixture matemático para verificar regras, não o balanceamento do jogo: duração de viagem de 10 segundos, k = 0,1, peso leve 1 e peso pesado 2. Exceto quando indicado, não há outros surgimentos.

| Cenário | Estado e entrada | Resultado esperado |
|---|---|---|
| Chegada sem macacos | Percurso 0, afundamento 0, carga 0; avançar 10 s | Arrived; afundamento continua 0; contador aumenta em 1 |
| Carga leve removida | Um leve ativo por 5 s; derrotá-lo e avançar os 5 s restantes | Afundamento fica em 0,5; não aumenta sem carga; Arrived no segundo 10 |
| Carga pesada mantida | Um pesado ativo desde o início | Afundamento chega a 1 no segundo 5; Sunk antes da chegada; sem relíquia |
| Carga pesada removida | Um pesado ativo por 2 s; aplicar dois toques para derrotá-lo e avançar até o segundo 10 | O peso permanece 2 após o primeiro toque e zera após o segundo; afundamento fica em 0,4; Arrived; uma relíquia |
| Empate exato | Um leve permanece ativo durante os 10 s | Afundamento chega a 1 junto com o fim do percurso; Arrived vence; uma única relíquia |
| Carga combinada | Um leve e um pesado permanecem ativos por 2 s | Peso total 3; progresso de afundamento aumenta em 0,6 |
| Pausa em segundo plano | Guardar qualquer estado e avançar 20 s com o aplicativo em segundo plano | Percurso, cronograma, carga e afundamento não mudam |
| Toque sobre alvos | Tocar dois alvos sobrepostos, um mais perto; repetir com centros à mesma distância | Um toque reduz apenas os toques restantes do alvo mais próximo; empate seleciona o mais antigo; a carga só cai quando esse alvo for derrotado |
| Surgimento no fim | Grupo agendado em 100% do percurso | A chegada encerra a tentativa antes do grupo; nenhum macaco nasce após Arrived |
| Retry após falha | Sunk com duas relíquias e cronograma atual de índice 1; executar Retry | Nova tentativa começa no índice 1, com percurso e afundamento em zero, sem macacos, mantendo duas relíquias e sem conceder recompensa |
| Próxima viagem após sucesso | Uma relíquia, cronograma atual de índice 0; chegar à ilha e então executar Continue | A chegada concede uma relíquia e deixa duas no total, avança uma vez ao índice 1 e aguarda; Continue inicia índice 1 com percurso e afundamento em zero, sem macacos e sem conceder recompensa |
| Quebra da maldição | Campanha com `requiredRelics = 3` e duas relíquias; chegar à ilha | Uma relíquia é concedida uma vez; estado da campanha torna-se CurseBroken; não há próxima viagem nem Continue |
| Meta configurada de uma relíquia | Campanha com `requiredRelics = 1` e zero relíquias; chegar à ilha | Uma relíquia é concedida uma vez; estado da campanha torna-se CurseBroken; não há próxima viagem nem Continue |
| Cronograma aleatório | Criar sessão com seed `-1` | Quantidades e classes coincidem com o cronograma; cada horário e posição fica nos limites configurados |
| Seed fixo | Criar duas definições com o mesmo seed não negativo | Horários e posições sorteados coincidem exatamente |
| Padrão no retry | Chegar a Sunk depois de ondas e executar Retry | Retry repete horários e posições sorteados, mantendo quantidades e classes |
| Nova sessão aleatória | Criar duas sessões com seed `-1` | Cada uma sorteia um cronograma novo, sem alterar quantidades ou classes |

## Revisão pendente

- O usuário atualizou em 2026-11-16 as regras: duração configurável entre 60 e 300 segundos; cada classe tem campo de toques configurável junto ao peso, com progressão crescente (configuração atual: leve 1, pesada 2). Mantêm-se os valores atuais de 180 segundos por viagem no asset e a composição fixa de cada onda.
- O usuário aprovou em 2026-11-16 o baseline Android e sua configuração no Inspector: duração configurável de 60–300 segundos (novos cronogramas começam em 300; a configuração atual usa 180 por viagem), `k = 0,0015`, as composições de ondas candidatas, raios de toque e sorteio limitado de horários/posições. Quantidades e classes permanecem fixas por onda.
- Compilação/importação no Unity Editor e validação de toque/balanceamento em aparelho permanecem manuais; este corte não executa build, testes ou Play Mode.

