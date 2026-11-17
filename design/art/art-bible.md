# Art Bible: Macacos me mordam

> **Status**: Draft
> **Owned By**: art-director
> **Last Updated**: 2026-11-16
> **Art Director Sign-Off (AD-ART-BIBLE)**: [Not yet reviewed]

`$art-bible` writes these nine sections in this order and replaces each
section's `[To be designed]` line as that section is approved. A section still
holding its placeholder is incomplete; a later run fills only those.

## 1. Visual Identity Statement

**Regra visual:** Cada viagem deve parecer uma página horizontal de aventura marítima pintada à mão, com formas caricatas e coloridas; a ameaça sobrenatural precisa ser reconhecida num relance, sem perder o humor.

- **Silhueta antes de detalhe.** O capitão, o barco e os macacos precisam continuar distinguíveis em tamanho reduzido, para o jogador encontrar alvos rapidamente. **Teste:** se adereços e texturas dificultarem reconhecer um macaco tocável, simplificar o detalhe ao redor. **Ancoragem:** toque rápido e leitura clara do perigo no pitch do MVP.
- **Comédia acompanhada por perigo legível.** Poses expressivas e proporções brincalhonas coexistem com um sinal sobrenatural claro. **Teste:** se o macaco parecer somente engraçado, reforçar um indício da maldição sem tornar a cena ameaçadora demais ou visualmente carregada. **Ancoragem:** aventura cômica sobrenatural e salvamento do capitão amaldiçoado.
- **Composição ampla com leitura rápida.** O navio e o mar formam o palco horizontal; o convés e os pontos onde os macacos se penduram permanecem visíveis e separados. **Teste:** se alvos, controles ou HUD competirem pela atenção ou encobrirem a ação, simplificar ou reposicionar. **Ancoragem:** jogo mobile em paisagem, com vários macacos e espaço para gestos e futuros controles laterais.

**Orientação:** paisagem é a direção visual aprovada para a tela de navegação. A largura deve acomodar o navio, os alvos e os controles laterais sem encobrir a ação; nenhuma configuração de Player Settings foi alterada.

**Referências:** usar qualidades gerais de aventura ilustrada em 2D e formas coloridas de leitura rápida, com identidade própria; não reproduzir personagens, composições, logotipos ou assets de *The Curse of Monkey Island* ou *Zack & Wiki*.

## 2. Mood & Atmosphere

O clima mantém a aventura cômica em primeiro plano. A magia aparece como um sinal sobrenatural claro, sem transformar o jogo em horror.

| Estado | Alvo emocional | Luz e contraste | Descritores | Energia |
|---|---|---|---|---|
| Preparação e navegação | Curiosidade despreocupada | Luz quente de manhã, contraste médio e cores abertas | ensolarado, salgado, curioso, aventureiro | Viva, mas controlada |
| Ataque dos macacos | Diversão sob pressão | Contraste alto; a luz quente do convés recebe acentos frios de magia | travesso, caótico, elétrico, ameaçador | Frenética |
| Afundamento iminente | Urgência com desespero cômico | Sombras azuladas crescem junto à água; as silhuetas continuam separadas | instável, molhado, tenso, absurdo | Urgente |
| Chegada, recompensa e Continue | Alívio e conquista | Clarão quente sobre ilha e relíquia; contraste forte na recompensa, mais suave na pausa | triunfante, luminoso, festivo, arejado | Explosão breve, seguida de calma |
| Afundamento e Retry | Revés recuperável | Cena fria e escurecida, com Retry em alto contraste | encharcado, frustrante, cômico, recuperável | Baixa, com chamada clara para agir |
| Maldição quebrada | Catarse e libertação | A luz dourada da aventura se abre; o violeta da maldição perde espaço | mágico, radiante, livre, exuberante | Alta e celebratória |

## 3. Shape Language

A linguagem mantém a leitura caricata e imediata da Seção 1. A tela de navegação usa composição horizontal; a posição e a hierarquia dos elementos da HUD estão descritas na Seção 7.

- **Formas principais:** capitão, macacos, navio e tesouro usam silhuetas amplas, contornos fortes e os detalhes mais expressivos. As classes de macacos diferem por proporção e postura: a leve é compacta e ágil; a pesada, larga e baixa. **Teste:** se um personagem ou alvo não for identificável em tamanho pequeno, simplificar sua silhueta antes de acrescentar detalhes.
- **Formas de apoio:** cordame, tábuas, barris, ondas e elementos distantes têm contornos mais simples, contraste menor e menos detalhes. O convés permanece visualmente aberto; céu, mar e ilhas formam camadas verticais. **Teste:** se um elemento do cenário competir com um personagem ou alvo tocável, reduzir sua ênfase visual.
- **Gramática da interface:** painéis e marcadores compartilham a aparência ilustrada do mundo; alertas podem usar contornos mais agudos e recompensas, formas fechadas e radiantes. Isso define pistas visuais sem fixar a composição da HUD. **Teste:** se estados ou marcadores forem confundidos sem cor, diferenciá-los também por contorno ou símbolo.
- **Foco da chegada:** o capitão alcança uma ilha pequena; um baú aberto revela a relíquia e se torna o foco visual da recompensa. Ilha, baú e relíquia precisam manter silhuetas separáveis em tela pequena. **Teste:** se a relíquia não for percebida dentro do baú num relance, simplificar o cenário e reforçar sua forma.

## 4. Color System

**Paleta inicial de sete cores**

| Cor | Uso e significado |
|---|---|
| Creme de vela `#F4E1B6` | Luz, áreas neutras e fundos de interface; mantém espaço visual para o texto. |
| Azul-petróleo `#2A91A1` | Mar e navegação; estabelece o ambiente marítimo tropical. |
| Azul-tinta `#20344D` | Contornos, sombras e texto; sustenta contraste e legibilidade. |
| Madeira envelhecida `#8A5A3B` | Casco e convés; ancora a cena pirata. |
| Coral de perigo `#E65F51` | Alertas e perigo imediato. |
| Violeta de maldição `#7351A2` | Bruxa, magia e sinais sobrenaturais. |
| Âmbar de tesouro `#F2B84B` | Relíquias, recompensa e progresso positivo. |

**Uso semântico e interface:** perigo combina coral com ícone ou padrão de alerta; maldição combina violeta com símbolo próprio; recompensa usa âmbar e a silhueta da relíquia. A HUD usa texto azul-tinta sobre creme ou painel escuro uniforme; evitar texto pequeno diretamente sobre fundos ilustrados.

**Acessibilidade:** nenhuma informação depende só da cor. Afundamento, perigo e recompensa também usam formas, contornos, padrões ou ícones. As classes de macaco se distinguem por silhueta e proporção, não apenas por matiz.

## 5. Character Design Direction

### Capitão

- **Arquétipo e silhueta:** um capitão pirata idoso, muito baixo e robusto, de formas arredondadas. A barba branca volumosa e o chapéu emolduram o rosto; o tapa-olho, as bochechas coradas e a perna de pau devem continuar reconhecíveis em tamanho pequeno.
- **Cores:** azul-escuro dominante na roupa, vermelho nos detalhes e bochechas em tom quente. A combinação de azul e vermelho e a silhueta do estudo ilustrado foram aprovadas; acessórios extras da exploração ainda não são parte do conceito.
- **Aparições:** cutscenes de abertura e encerramento e sprite do capitão no navio durante a viagem. Ele não aparece no pop-up da ilha.
- **Personalidade em cena:** a avareza por tesouros e o comportamento tarado e mulherengo são falhas cômicas que motivam a maldição. Olhares cobiçosos e mãos agarradas a moedas ou relíquias mostram a ganância; pose convencida e paquera exagerada mostram seu lado mulherengo. Sob ataque, a autoconfiança vira pânico cômico.
- **Acabamento e leitura:** contornos limpos, áreas de cor simples e poucos detalhes pequenos. Priorizar a leitura do rosto, do tapa-olho, da barba e da perna de pau à distância de jogo em tela mobile.

### Macacos demoníacos

A arte de gameplay mantém contornos limpos, áreas de cor simples e poucos detalhes pequenos, no mesmo nível de limpeza do capitão. A silhueta, os chifres, as caudas, a postura e a paleta distinguem os tipos em tela pequena; a cor nunca é o único sinal.

**Escopo:** o protótipo atual usa os Tipos 1–2; o MVP incluirá os Tipos 1–3; a versão final prevê os cinco tipos. Os Tipos 4–5 são variantes especiais planejadas para depois do MVP.

**Estudo detalhado:** [Folha exploratória dos cinco tipos](concepts/demon-monkeys-cutscene-study.png). Esta arte detalhada fica como referência para possíveis cutscenes; não é um asset final nem o padrão visual de gameplay.

- **Tipo 1 — leve e arteiro:** o menor e mais esguio, com chifres pequenos, expressão travessa e cor de assinatura vermelho-amarronzada. É a classe mais leve; o valor-base atual é um toque para derrotá-lo.
- **Tipo 2 — agressivo:** mais forte e com aparência mais próxima de um macaco, pelagem preto-azulada, expressão agressiva, chifres salientes e duas caudas. O valor-base atual é de dois toques.
- **Tipo 3 — pesado:** corpo grande e arredondado, próximo de um orangotango, expressão preguiçosa, cores laranja e azul, chifres enrolados como os de carneiro e sem cauda. É mais pesado e exige mais toques que o Tipo 2.
- **Tipo 4 — especial, teleporte:** lembra um mico-leão-dourado, em verde-musgo e dourado. Cada toque recebido o transporta para outro ponto do navio. Um rastro de teleporte é apenas uma possibilidade visual; o efeito final será definido depois.
- **Tipo 5 — especial, alado:** lembra um babuíno e tem asas de morcego negras ou azul-escuras. Voa livremente sem pousar fisicamente no navio, mas a maldição ainda faz seu peso afetar a embarcação.

Os pesos e as quantidades de toques permanecem configuráveis por classe.

### Bruxa do mar

- **Papel e acabamento:** aparece apenas em cutscenes, possivelmente em diálogos. A versão apresentada no jogo mantém o traço limpo e as formas simples do capitão; estudos detalhados servem como referências para possíveis diálogos e cutscenes, não como arte final.
- **Forma de sereia:** mulher adulta, bela e curvilínea, com cauda de sereia e silhueta fluida.
- **Forma de bruxa:** idosa, baixa e gordinha, com nariz grande, salto alto, vestido e chapéu pontudo.
- **Ligação entre formas:** roxo e lilás são as cores comuns; preto pode ser usado como apoio. Nenhum acessório ou símbolo compartilhado foi definido.
- **Expressão:** há um estudo de rosto furioso e exagerado para cada forma, com atuação teatral para possíveis diálogos e cutscenes.

**Estudos detalhados:** [Duas formas](concepts/sea-witch-dual-form-cutscene-study.png) · [Expressões furiosas](concepts/sea-witch-fury-expression-study.png). São referências exploratórias; adornos e detalhes dessas imagens ainda não são decisões finais.

## 6. Environment Design Language

- **Composição e silhueta do navio:** câmera quase lateral, com o navio maior e imponente como palco principal. O barco terá exatamente três mastros altos com velas sobrepostas, um casco largo, popa elevada e robusta e um gurupés comprido e elegante na proa. O enquadramento mostra convés, velas, cordame, casco, proa e popa como áreas distintas onde os macacos possam se pendurar.
- **Composição da tela de navegação:** orientação paisagem. O navio marrom, em vista quase lateral, ocupa a região central e a maior parte da largura, com três mastros, proa e popa visíveis. A faixa azul do mar começa na parte baixa do casco e ocupa a região inferior; uma camada translúcida à frente do casco comunica o afundamento. A HUD fica no topo, sem moldura fixa; na base, o diálogo fica centralizado sobre a água e há espaço para um canhão em cada canto inferior. Esses elementos não devem encobrir o navio nem os alvos tocáveis.
- **Hierarquia visual:** capitão e macacos são os elementos mais fáceis de localizar. Os pontos de apoio dos macacos precisam se separar visualmente; cordas, velas e casco usam formas limpas e pouco ruído ao redor dos alvos tocáveis.
- **Fundo:** mar, céu e ilha de destino comunicam a viagem, mantendo menos contraste e detalhes que o navio e os personagens.
- **Acabamento:** formas cartunescas pintadas, contornos claros e textura contida, alinhados à direção visual do capitão e à leitura rápida aprovada.
- **Referência e escopo aberto:** a imagem compartilhada guia a câmera lateral e a silhueta do navio. Caveiras, penhascos, céu noturno e outros detalhes das explorações não são requisitos confirmados. O arranjo exato das velas, detalhes de construção e a composição da tela de chegada permanecem em aberto.

### Coleção de relíquias

- **Temas e motivos:** joias e pequenos objetos preciosos inspirados em pirataria, navegação e praia. Âncora, concha, arpão, peixe, papagaio e gancho de pirata são motivos candidatos, sem fechar a lista final.
- **Coesão visual:** as peças devem parecer uma coleção pelo uso de uma paleta marítima próxima e pelo acabamento consistente; cada uma mantém uma silhueta própria. Âmbar/ouro pode dominar várias peças, acompanhado por azuis e turquesas, creme perolado e pequenos acentos de coral.
- **Materiais:** algumas relíquias podem combinar ouro com pedras preciosas ou pérolas; não é necessário que todas usem os mesmos materiais.
- **Leitura:** cada joia precisa ser reconhecível isoladamente e dentro do baú aberto, com formas limpas e poucos detalhes pequenos.

**Estudo ilustrado:** [Coleção exploratória de joias](concepts/relic-jewelry-collection-study.png). A folha demonstra motivos e coesão visual; não define quantidade, ordem, raridade ou progressão das relíquias.

### Baú do tesouro

- **Forma e acabamento:** baú robusto, de formas simples e com pouca decoração.
- **Motivos:** ornamentos escolhidos entre os temas da coleção de relíquias ligam o baú aos tesouros; os motivos exatos permanecem em aberto.
- **Ligação com a bruxa:** o fecho usa violeta e lilás. Dourado e detalhes náuticos mantêm o baú dentro da mesma coleção visual.
- **Revelação:** na chegada à ilha, o baú aberto revela a relíquia como foco da recompensa.

**Estudo ilustrado:** [Baú com ligação visual à bruxa](concepts/treasure-chest-witch-link-study.png). A imagem é exploratória; o desenho final do fecho e os ornamentos ainda não foram definidos.

### Ilha do tesouro

- **Forma e leitura:** ilha pequena e simples, com palmeira, grama e arbustos.
- **Uso:** a mesma linguagem visual atende ao ícone de HUD e a uma versão maior na tela de chegada; a composição dessa tela permanece em aberto.

**Estudo ilustrado:** [Ícone e ilha para chegada](concepts/treasure-island-icon-study.png). A folha é exploratória; elementos adicionais nela não são decisões finais.

## 7. UI/HUD Visual Direction

- **Apresentação:** HUD sobre a tela, sem moldura fixa no topo. Uma linha horizontal representa o trajeto; o marcador do barco, representado por uma seta no esquema, avança conforme o tempo ativo da viagem. Um ícone de baú marca o destino no fim da linha.
- **Marcadores de relíquia:** a campanha determina a quantidade; há exatamente um marcador abaixo da linha para cada relíquia exigida. Os adquiridos usam âmbar e se distinguem dos não adquiridos também por forma, contorno ou símbolo, sem depender só da cor. O tratamento exato dos ainda não adquiridos permanece em aberto.
- **Diálogos e tutoriais:** a caixa de diálogo fica centralizada na faixa inferior sobre a água; o retrato expressivo do personagem aparece à esquerda e as linhas de texto à direita. Ela não cobre visualmente o navio ou os macacos e preserva água visível para comunicar o afundamento. Mensagens de contexto surgem e desaparecem sem pausar a viagem. Tutoriais de macacos ou mecânicas incluídos na versão podem pausar a viagem e as ações; instruções de agarrar e canhões ficam para versões futuras que incluam esses recursos.
- **Canhões (versão futura):** há espaço para dois canhões, um em cada canto inferior. Eles não fazem parte do MVP atual.
- **Chegada e recompensa:** a configuração da campanha define quantas relíquias são necessárias. Após cada chegada, abre-se um pop-up celebratório sobre o jogo, inspirado em jogos mobile, com a navegação visível e escurecida ao fundo. A ilha e o baú ocupam o lugar das estrelas; o baú começa fechado, abre com efeitos de comemoração e revela a relíquia. O pop-up mostra quantas relíquias ainda faltam. Se o total configurado ainda não foi alcançado, exibe Continue para avançar; ao alcançar o total, apresenta o final da maldição e não oferece Continue. Toda a sequência acontece dentro do pop-up.
- **Afundamento:** o navio afunda visualmente no cenário, visto através de uma camada translúcida de água. Quando o casco fica submerso, começa a animação de naufrágio e a tripulação se joga ao mar. No MVP, não há medidor separado na HUD; o barco e as reações do capitão comunicam o perigo.
- **Capitão no navio:** o jogador não consegue afetá-lo. Conforme o navio se aproxima de afundar, as animações do sprite ficam mais desesperadas.

**Ainda em aberto:** tipografia, tamanhos e hierarquia de leitura, estilo final dos ícones, duração e ritmo dos efeitos.

## 8. Asset Standards

- **Fonte editável:** cada asset 2D mantém seu arquivo-fonte editável no formato nativo da ferramenta usada para criá-lo.
- **Export para Unity:** sprites são exportados como PNG; usar transparência quando o elemento precisar de fundo transparente.
- **Nomes:** arquivos recebem nomes descritivos em inglês.
- **Definição por tela e aparelho:** resolução final e Pixels Per Unit serão definidos com a primeira tela e os aparelhos-alvo.
- **Atlas e limites de importação:** atlas, compressão e limites técnicos de textura serão definidos com um conjunto inicial de assets e a plataforma-alvo.
- **Escopo atual:** padrões para assets 2D; LOD e orçamentos de polígonos 3D não se aplicam ao escopo atual.

## 9. Reference Direction

- *The Curse of Monkey Island*: referência para aventura 2D pintada à mão e atuação cômica expressiva. Evitar copiar personagens, cenários, silhuetas e composições reconhecíveis.
- *Zack & Wiki*: referência para formas coloridas e leitura clara de uma aventura estilizada. Evitar reproduzir personagens, itens, cenários ou interface.
- **Referência de navio enviada pelo usuário:** aproveitar a câmera lateral e a presença de um navio imponente. O barco do jogo mantém identidade própria e os três mastros definidos; evitar copiar a silhueta exata, os ornamentos e o cenário da imagem.
- **Referências de diálogo enviadas pelo usuário:** aproveitar a combinação de retrato expressivo com caixa de texto legível. Criar personagens e interface próprios, sem reproduzir os desenhos ou o layout das telas.
- **Referência de recompensa mobile enviada pelo usuário:** aproveitar a hierarquia de uma celebração com a recompensa no foco. Usar ilha, baú e relíquia no lugar de estrelas; não reproduzir o layout, os ícones ou os elementos de marca da imagem.

**Direção combinada:** misturar o acabamento ilustrado e a atuação expressiva com formas simples e leitura rápida durante o jogo. As referências orientam qualidades gerais; nenhuma deve ser copiada literalmente.
