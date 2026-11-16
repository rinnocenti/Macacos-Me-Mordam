# Macacos me mordam

Jogo mobile 2D de arcade frenético sobre um capitão pirata amaldiçoado e macacos demoníacos que tentam afundar seu navio.

## Estado do projeto

- Unity 6000.6.9f1, C# e URP.
- Android; Touch é a entrada principal, com suporte completo a toque. Gamepad: nenhum.
- O Build Profile Android está ativo no Unity; ainda não foi gerado APK.
- Este repositório contém a raiz do projeto Unity Universal 2D (URP).
- O projeto usa o checkout local `com.immersive.framework` 1.2.0; o lock resolve Composition 0.1.0-preview.3, Foundation 0.2.3 e Logging 0.2.4.
- O Unity Test Runner mostrou 22/22 testes NUnit EditMode aprovados na execução feita pelo usuário.

## Base Immersive Framework

O jogo consome o checkout local do pacote `com.immersive.framework` 1.2.0 por `file:../../../../../ImmersivePackages/com.immersive.framework`. O checkout do pacote precisa existir nessa posição relativa ao manifest do jogo; ele não está incluído neste repositório. A Unity Package Manager aceita caminhos locais relativos ao manifest do projeto.

O Framework é uma dependência do jogo. Não copie código ou arquitetura interna do pacote para este repositório. As regras próprias do jogo — viagens, surgimento e derrota dos macacos, peso, afundamento e coleta de relíquias — pertencem ao consumidor. A recompilação no Editor concluiu sem erros; o usuário confirmou a demonstração do loop e 22 testes EditMode aprovados. Não foi gerado APK nem validado em aparelho. A diferença entre Cinemachine 3.1.7 declarada pelo checkout local e 6.6.0 resolvida pelo lock permanece sem validação específica de runtime.

## Próxima etapa

A demonstração mínima da campanha foi aceita pelo usuário. O próximo incremento é melhorar a apresentação mobile, começando pela legibilidade e exibição dos textos localizados da HUD em diferentes proporções de tela; em seguida, definir direção de arte e substituir os visuais temporários. O build Android e a validação em aparelho continuam pendentes.

## Documento de design

O conceito e o escopo inicial estão em design/game-brief.md.
