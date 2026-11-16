# Proposta de balanceamento: viagens Android

**Tipo:** Tuning
**Sistema:** Voyage
**Status:** Baseline aprovado pelo usuário em 2026-11-16; configurado para playtest, ainda sem validação em aparelho.
**GDD de referência:** `design/gdd/voyage-system.md` — Parâmetros de balanceamento
**Data:** 2026-11-16

## Objetivo

Registrar o baseline de duração, ondas, afundamento e toque para a campanha Android de três relíquias. Quantidades e classes de cada onda são fixas. Horários e posições são sorteados nos limites configurados e ficam estáveis durante a sessão: Retry repete exatamente o padrão; uma nova sessão sorteia outro quando o seed é `-1`.

## Valores propostos

| Parâmetro | Valor aprovado ou atual | Proposta para revisão | Racional |
|---|---|---|---|
| Duração de cada viagem | Não definida anteriormente | **60–300 s configuráveis; novo cronograma começa em 300 s (5 min)** | Mantém o percurso como relógio da tentativa. Uma campanha perfeita dura 3–15 min, sem contar retries; o asset atual mantém cada viagem em 180 s. |
| Taxa de afundamento `k` | Não definida; `0,1` é somente fixture | **0,0015** por unidade de peso por segundo; faixa de comparação de playtest: `0,0010–0,0020` | Com um macaco pesado ativo por 300 s, o valor inicial acumula 90% de afundamento; com um leve, 45%. |
| Classe Leve | Peso 1 aprovado | Manter peso 1 | Baseline existente no GDD. |
| Classe Pesado | Peso 2 aprovado | Manter peso 2 | Baseline existente no GDD e prioridade de clique. |
| Toques para derrotar | Um toque para todas as classes | Campo **Cliques para derrotar** configurável ao lado do peso; valores iniciais classe 1 = 1, classe 2 = 2 | A classe permanece no navio com o peso completo até o toque final; classes mais pesadas devem exigir mais toques. |
| Raio de toque | Não definido | Leve: **64**; Pesado: **80** unidades locais do tabuleiro | Alvo pesado um pouco maior para facilitar o toque prioritário. As unidades dependem da escala do Canvas e não equivalem diretamente a dp. |
| Horário de cada onda | Não definido | Progresso nominal configurado com variação inicial de **±3 s**; valor ajustável no Inspector por onda | Cada janela deve ficar após a partida, antes da chegada e sem sobrepor ou reordenar as janelas vizinhas. |
| Área de surgimento | Não definida | Centro por ocorrência com retângulo de variação configurável; semi-extensão inicial **40 × 24** unidades locais | Cada macaco sorteia uma posição dentro da área limitada pelo centro e pelas semi-extensões. Coordenadas devem ficar dentro do convés e fora da HUD conforme o layout. |
| Seed | Não definido | **-1** para novo padrão por sessão; seed não negativo para repetir uma amostra | O seed fixo facilita comparar ajustes; Retry sempre conserva a amostra da sessão. |

## Cronogramas candidatos

Notação: `L` = Leve; `P` = Pesado. Cada item da sequência é a composição de uma onda. Os tempos listados correspondem ao progresso nominal vezes a duração inicial de 300 segundos; ao ajustar a duração, os instantes escalam na mesma proporção e o sorteio temporal varia dentro dos limites configurados. As ondas começam depois da partida e terminam antes da chegada.

| Viagem | Instantes das ondas | Composição por onda | Total |
|---|---|---|---|
| 1 | 15 s e depois a cada 30 s; última aos 285 s | `2L, 2L, LP, 2L+P, 2L+P, 2L+P, 2L+P, 3L+P, 3L+P, 3L+P` | 30: 22L + 8P |
| 2 | 12 s e depois a cada 24 s; última aos 276 s | `LP, 2L, 2L+P, L+2P, 2L+P, L+2P, 2L+2P, 2L+2P, 2L+2P, 2L+2P, 2L+3P, 2L+3P` | 42: 21L + 21P |
| 3 | 10 s e depois a cada 20 s; última aos 290 s | `3L, 2L+P, 2L+P, L+2P, 2L+2P, 2L+2P, 2L+2P, L+3P, L+3P, 2L+3P, L+4P, L+4P, L+4P, L+4P, 3L+3P` | 63: 25L + 38P |

O baseline soma **135 macacos** nas três viagens. A cadência e a participação de Pesados aumentam a cada ilha. A tabela mostra os progressos nominais: o horário real varia em ±3 segundos por onda. Nenhuma onda surge em 100% do percurso; quantidades e classes permanecem invariáveis ao sorteio.

## Mapeamento e critérios de aceitação

- Duração, taxa, seed, variação temporal, centros e semi-extensões alimentam `CampaignDefinitionAsset`; cada duração valida entre 60 e 300 segundos.
- Cada tentativa sorteia horário e posição dentro dos limites; a variação temporal efetiva é reduzida quando necessário para manter cada onda depois da partida e antes da chegada. O sistema respeita a pausa do aplicativo e não cria macacos depois de `Arrived` ou `Sunk`.
- Quantidade e classe por onda nunca mudam com o sorteio. Retry repete a amostra e Continue usa a próxima amostra já armazenada.
- As posições locais do tabuleiro ficam dentro do convés e não cobrem controles da HUD.
- Cada toque atinge no máximo um alvo; `clicksToDefeat` configura os toques por classe e seu peso só sai da carga ao ser derrotada. Sobreposição continua resolvida pela regra já aprovada de distância e ordem de surgimento.
- Em dispositivo Android, revisar o tamanho real dos alvos táteis e a dificuldade; os raios em unidades locais não substituem validação visual.

## Aprovação registrada

O usuário aprovou o baseline e solicitou que os valores sejam configuráveis no Inspector, incluindo duração por viagem entre 180 e 300 segundos. Em ajuste posterior, ampliou a faixa para 60–300 segundos e pediu que os toques necessários sejam configuráveis junto ao peso da classe; os valores atuais permanecem um para Leve e dois para Pesado. A quantidade e a classe de cada onda ficam fixas; horário e posição são aleatórios dentro de limites definidos. Falta validar a dificuldade, os alvos táteis e as áreas no Unity em aparelho Android.
