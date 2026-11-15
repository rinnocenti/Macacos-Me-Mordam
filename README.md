# Macacos me mordam

Repositório local do jogo mobile 2D de arcade frenético sobre um capitão pirata amaldiçoado e macacos demoníacos que tentam afundar seu navio.

## Estado do projeto

- Unity 6000.6.9f1, C# e URP.
- Mobile; Touch é a entrada principal, com suporte completo a toque. Gamepad: nenhum.
- O sistema operacional móvel ainda não foi escolhido.
- O projeto Unity ainda não foi criado. Este repositório contém o brief aprovado e as decisões iniciais.
- NUnit é o framework de testes pretendido.

## Base Immersive Framework

O jogo consumirá o pacote oficial com.immersive.framework na versão estável 1.1.0, distribuída pelo OpenUPM. A documentação do pacote registra Unity 6000.5.0f1 como versão mínima; o projeto escolhido usa 6000.6.9f1, acima desse mínimo. A importação e compilação no projeto consumidor ainda precisam ser verificadas depois que o projeto Unity for criado.

O Framework será uma dependência do jogo. Não copie código ou arquitetura interna do pacote para este repositório. As regras próprias do jogo — viagens, surgimento e derrota dos macacos, peso, afundamento e coleta de relíquias — pertencem ao consumidor. A configuração do registro OpenUPM e da dependência será feita no manifest do projeto Unity quando ele existir.

## Próxima etapa

Criar o projeto Unity 2D URP pelo Unity Hub dentro deste repositório e, em seguida, configurar a dependência estável do Immersive Framework. Não há cenas, scripts ou arquivos de projeto Unity neste commit.

## Documento de design

O conceito e o escopo inicial estão em design/game-brief.md.
