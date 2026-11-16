# Design: Activity de campanha e ciclo de viagem

**Status:** Fronteira e regras centrais aprovadas pelo usuário.

**Data:** 2026-11-15
**Fonte:** [Brief do jogo](../../../design/game-brief.md)

## Intenção

Definir os limites entre o Immersive Framework e as regras do arcade antes de criar scripts ou cenas de gameplay. O MVP precisa repetir viagens curtas: navegar, repelir macacos, chegar à ilha ou afundar, e então tentar de novo ou avançar.

## Objetivos

- Manter o Framework responsável pela aplicação e pelo fluxo amplo de navegação.
- Manter no jogo o estado da campanha e as regras específicas de cada viagem.
- Permitir que uma Activity de campanha contenha várias tentativas de viagem.
- Deixar fórmulas e valores de balanceamento em aberto para o desenho do sistema.

## Fora do escopo

- Criar código, cenas, prefabs ou assets.
- Definir duração, tabela de surgimento, pesos, fórmula de afundamento ou quantidade de relíquias.
- Criar salvamento persistente, regiões adicionais, economia ou monetização.
- Modificar o código do Framework ou usar contratos Experimentais além do mínimo de câmera comprovadamente exigido para o boot.

## Abordagens consideradas

1. **Uma Activity por viagem.** Cada tentativa teria seu próprio ciclo de entrada e saída no Game Flow. Isso deixa os limites de ciclo de vida explícitos, mas faz uma tentativa repetível depender mais das transições e do reset de Activities.
2. **Uma Activity de campanha com viagens como estado do jogo — escolhida.** A aplicação do Framework abre uma Route e uma Activity de campanha. O jogo repete a viagem dentro dessa Activity e decide o resultado de cada tentativa. Isso mantém navegação separada das regras do arcade e evita transformar cada retry em uma mudança de Activity.

O usuário escolheu a segunda abordagem em 2026-11-15.

## Desenho proposto

O Framework fornece o limite de aplicação e navegação: aplicação/sessão, uma Route da campanha e uma Activity da campanha. O jogo mantém o progresso de relíquias e o cronograma atual da campanha, além do estado da tentativa.

- **Progresso da campanha:** relíquias obtidas, índice do cronograma atual e condição para quebrar a maldição.
- **Tentativa de viagem:** avanço do percurso, macacos ativos e seus pesos, condição do navio e resultado da tentativa.

Enquanto a viagem está ativa, o percurso funciona como temporizador. Macacos ativos contribuem para a carga do navio; o toque do jogador pode removê-los. A tentativa termina quando o navio afunda ou o percurso chega à ilha. Uma falha concede zero relíquias e permite retry no mesmo cronograma, resetando somente o estado da tentativa. Uma chegada concede uma relíquia exatamente uma vez e avança ao cronograma seguinte. Abaixo de três relíquias, a campanha aguarda uma ação explícita Continue antes de iniciar a viagem seguinte; Continue não concede recompensa. Na terceira relíquia, a campanha termina com a maldição quebrada. O formato da apresentação do resultado permanece em aberto.

Entrada por toque, objetos visuais, animações, HUD e feedback audiovisual apresentam o estado do jogo. Eles não assumem autoridade sobre peso, afundamento, resultado ou progresso. As regras permanecem em código e dados pertencentes ao jogo; o Framework não recebe mecânicas de macacos, navio ou recompensa.

## Limite entre regras e apresentação

O estado da viagem fornece os resultados do gameplay e atualiza o progresso da campanha. A apresentação consome esse estado para desenhar o navio, os macacos e a HUD; o toque envia uma intenção para as regras do jogo. Esta é uma fronteira arquitetural preliminar, não um mapa completo de sistemas ou dependências.

## Restrições e maturidade

- O projeto usa Unity 6000.6.9f1, entrada Touch e suporte completo a toque; o sistema operacional móvel ainda está em aberto.
- O manifesto local aponta para uma cópia de Immersive Framework fora deste repositório. O package.json dessa cópia declara a versão 1.2.0, mas a revisão Git local não foi confirmada. Composition 0.1.0-preview.3, Foundation 0.2.3 e Logging 0.2.4 estão resolvidos pelo lockfile. A documentação instalada marca a autoria de Application, Route e Activity e os pedidos de fluxo como Stable; a maturidade continua específica por API.
- O manifesto usa um caminho absoluto específico desta máquina; essa alteração ainda não está commitada e não deve ser publicada como configuração portátil.
- O Framework declara Cinemachine 3.1.7, enquanto o lock do projeto resolve Cinemachine 6.6.0 como builtin. A compatibilidade entre essa resolução e o Framework não foi confirmada por importação/compilação no Unity.
- O caminho de câmera exigido pelo bootstrap depende de Camera Session, CameraOutputDefinition e FixedCameraRigBehaviorDefinition, todos Experimentais na superfície necessária. O usuário aceitou esse risco para o MVP; importação e compilação ainda precisam ser verificadas no Unity, inclusive a divergência entre Cinemachine 3.1.7 declarado pelo Framework e 6.6.0 resolvido pelo projeto.
- Nenhuma compilação ou execução Unity foi observada nesta sessão.
- O mapa completo de sistemas, dependências e prioridades continua pendente; a auditoria do bootstrap do Framework está registrada separadamente.

## Questões ainda abertas

- Se a cena da viagem também apresenta resultados ou se haverá uma tela separada.
- Mapa completo de sistemas, dependências e prioridades do jogo.

