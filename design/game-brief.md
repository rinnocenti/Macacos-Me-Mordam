# Macacos me mordam — Brief do jogo

## Conceito

Um capitão pirata foi amaldiçoado por uma bruxa do mar. Sempre que navega em direção a uma ilha do tesouro, macacos demoníacos brotam no barco e tentam afundá-lo. O capitão precisa sobreviver às viagens e reunir os itens de tesouro necessários para se livrar da maldição.

## Direção

- **Gênero:** arcade frenético de toque e clique, 2D.
- **Plataforma:** Android no MVP; navegação em paisagem.
- **Entrada:** toque como controle principal, suporte completo a toque e sem gamepad.
- **Foco:** repelir hordas de macacos demoníacos e salvar o capitão amaldiçoado.
- **Tom:** aventura cômica com elementos sobrenaturais.
- **Público:** jogadores casuais que procuram sessões curtas e ação imediata.

## Ciclo principal

1. O barco parte rumo a uma ilha com um tesouro.
2. A distância até a ilha funciona como o temporizador da viagem.
3. Macacos demoníacos aparecem no barco em quantidades variáveis e com pesos diferentes.
4. O jogador toca nos macacos para derrotá-los antes que o peso acumulado afunde o navio.
5. Se o navio resistir até a ilha, o capitão obtém um tesouro ou relíquia e avança na jornada.
6. Depois de reunir a quantidade necessária de itens, o capitão quebra a maldição.

## Objetivo e falha

- **Objetivo:** alcançar ilhas do tesouro e coletar os itens que encerram a maldição.
- **Falha da viagem:** o navio afunda antes de chegar à ilha.
- Cada campanha define na configuração quantas relíquias são necessárias para quebrar a maldição; a configuração atual usa três.

## Escopo do MVP

1. Uma rota de viagem com uma ilha de tesouro.
2. Surgimento de macacos durante a viagem, com quantidade variável e três tipos de peso no MVP; o protótipo mecânico atual usa dois.
3. Toque para derrotar os macacos.
4. Macacos sobreviventes adicionam peso e aumentam o afundamento do navio.
5. Temporizador de viagem, estado do navio e feedback essencial na HUD.
6. Resultado de vitória ao chegar à ilha e de falha ao afundar, com opção de tentar novamente.
7. Recompensa de tesouro ou relíquia e contador de progresso para quebrar a maldição.

## Fora do escopo inicial

- Várias regiões, rotas, navios ou chefes.
- Melhorias, economia de longo prazo, multiplayer e monetização.
- Salvamento persistente de campanha.
- Tipos especiais 4 e 5, planejados para a versão final de cinco tipos.
- Áudio final.
- Valores finais de tempo, frequência de surgimento, peso e afundamento; a quantidade de relíquias é definida pela configuração de cada campanha.

## Apresentação

A direção visual 2D cartunesca, os personagens e a composição da navegação estão registrados em `design/art/art-bible.md`; os fluxos, estados e interações mobile estão em `design/ux/campaign-flow.md`. O áudio pode reforçar madeira rangendo, água entrando no barco e impactos cômicos ao derrotar inimigos; a identidade sonora ainda será definida.

## Ordem sugerida de construção

1. Validar o ciclo de viagem, toques, peso e afundamento.
2. Acrescentar a recompensa e o contador de relíquias.
3. Desenvolver apresentação visual e sonora.

## Referências

As referências visuais e os limites de inspiração estão registrados na Seção 9 de `design/art/art-bible.md`, incluindo *The Curse of Monkey Island* e *Zack & Wiki*.
