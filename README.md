# Macacos me mordam

Repositório local do jogo mobile 2D de arcade frenético sobre um capitão pirata amaldiçoado e macacos demoníacos que tentam afundar seu navio.

## Estado do projeto

- Unity 6000.6.9f1, C# e URP.
- Mobile; Touch é a entrada principal, com suporte completo a toque. Gamepad: nenhum.
- O sistema operacional móvel ainda não foi escolhido.
- O projeto Unity foi criado pelo Unity Hub em `Macacos Me Mordam/`, com o template Universal 2D (URP).
- O manifest declara `com.immersive.framework` 1.1.0 pelo OpenUPM; a resolução e a importação do pacote ainda não foram confirmadas.
- NUnit é o framework de testes pretendido.

## Base Immersive Framework

O jogo consome o pacote oficial `com.immersive.framework` na versão estável 1.1.0, distribuída pelo OpenUPM. A documentação do pacote registra Unity 6000.5.0f1 como versão mínima; o projeto escolhido usa 6000.6.9f1, acima desse mínimo. O manifest declara o registro e a dependência; a resolução e a compilação do pacote ainda precisam ser confirmadas no Editor.

O Framework é uma dependência do jogo. Não copie código ou arquitetura interna do pacote para este repositório. As regras próprias do jogo — viagens, surgimento e derrota dos macacos, peso, afundamento e coleta de relíquias — pertencem ao consumidor.

## Próxima etapa

Confirmar a resolução do Immersive Framework e suas dependências no Editor. Depois, definir a arquitetura mínima do ciclo de viagem antes de criar scripts de gameplay.

## Documento de design

O conceito e o escopo inicial estão em design/game-brief.md.
