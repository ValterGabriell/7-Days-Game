

# 📜 GAME DESIGN DOCUMENT (GDD)

## 📄 1. Visão Geral do Projeto

* **Título Provisório:** *Última Frequência* (ou *Frequência de Emergência*)
* **Gênero:** Simulação Narrativa / Gerenciador Distópico / Terror Psicológico
* **Plataforma:** PC (Godot 4)
* **Tom / Atmosfera:** Retro-futurista, claustrofóbico, melancólico, cinzento e claustrofóbico.
* **Premissa:** O mundo tem apenas 7 dias de existência antes do fim definitivo da Terra. O jogador é o único radialista da cidade operando em um estúdio isolado. Sua função é receber dados de várias fontes, decidir qual viés dará a cada notícia e transmiti-las ao público. O trabalho é sua mecânica de negação frente à tragédia inevitável.

---

## 🔁 2. Ciclo de Gameplay (Gameplay Loop)

1. **Início do Dia:** O radialista acorda no estúdio isolado.
2. **Triagem de E-mails (PC):** O jogador analisa e-mails de **Governo**, **Jornalistas**, **Ouvintes** e **Pedidos de Música**.
3. **Análise e Escolha de Viés:** As notícias possuem tom cinzento. O jogador escolhe a ação editorial para cada notícia:
* *Verdade Nua e Crua*
* *Omitir / Suprimir*
* *Mentir / Alterar*
* *Distorcer / Foco Crítico*

Sentar na Mesa: O jogador senta no PC para trabalhar.

Colocar Música para Tocar (Música de Fundo / Intervalo): Enquanto analisa as notícias e decide o tom de cada uma, o rádio precisa continuar tocando músicas para manter a programação no ar.

Decidir e Transmitir: O jogador marca a ação editorial de cada notícia e pode escolher transmitir uma por uma ou selecionar várias e transmitir em bloco.

Impacto: Ao clicar em transmitir, o som do microfone entra, o texto vai ao ar e o impacto na população (Audiência, Esperança, Irritação) é aplicado ao fim do dia!.


4. **Impressão e Transmissão:** O jogador imprime a pauta na impressora 3D da mesa e realiza a transmissão no microfone.
5. **Eventos Locais (Mesa/Porta):** Batidas na porta, chamadas de ouvintes ao vivo ou visitas da fiscalização acontecem durante o turno.
6. **Encerramento e Feedback das Consequências:** A tela escurece. O jogo processa as mudanças na população e exibe um relatório matutino de como as decisões afetaram as pessoas no dia seguinte.

---

## 📊 3. Sistemas Principais e Métricas

O estado da cidade é ditado por 3 métricas correlacionadas:

```text
			  ┌───────────────────────────┐
			  │    Nível de Audiência     │
			  └─────────────┬─────────────┘
							│
			┌───────────────┴───────────────┐
			▼                               ▼
  ┌───────────────────┐           ┌───────────────────┐
  │ Status: ESPERANÇA │           │ Status: IRRITAÇÃO │
  └───────────────────┘           └───────────────────┘

```

### 3.1. As Métricas da Sociedade

* **Pânico / Conforto:** Mede o nível de desespero imediato da população nas ruas.
* **Confiabilidade do Radialista:** A fé que a população e as instituições têm na sua palavra.
* **Audiência:** A quantidade de ouvintes sintonizados no programa.

### 3.2. As Duas Polaridades de Audiência Alta

* **Audiência Alta + Esperança (Manipulação Governamental):**
* **Efeito:** As empresas e a elite usam sua voz para acalmar as massas.
* **Resultado no PC:** O governo envia mais relatórios oficiais e maquiados.
* **Caminho Narrativo:** Mantém você seguro no cargo e abre a possibilidade de conseguir uma vaga na nave de evacuação para **Alpha Centauri** (*Seleção Natural*).


* **Audiência Alta + Irritação (Tensão Revolucionária):**
* **Efeito:** Notícias expõem a corrupção e a tragédia iminente, alimentando o ódio contra a elite.
* **Resultado no PC:** O povo envia vazamentos e denúncias ativas.
* **Caminho Narrativo:** Desencadeia revoltas e motins urbanos que podem impedir a nave *Seleção Natural* de decolar.



---

## ⚖️ 4. O Estado Neutro: Tensão Social Volátil (Paralisação)

Quando a audiência fica equilibrada no meio ($50\%$ Esperança / $50\%$ Irritação), a cidade entra num estado de paralisação e paranóia. É o cenário ideal para sobrevivência, mas o mais caótico no estúdio:

* **Clima Social:** A população não confia plenamente no governo nem tem força armada coesa. O programa vira a única fonte neutra e imprevisível.
* **Guerra de Fontes no PC:** O computador vira um campo de batalha. Notícias chegam adulteradas, e o governo tenta subornar o jogador enquanto os rebeldes vazam arquivos sigilosos.
* **Ameaça Dupla:**
* *Governo:* Envia fiscais ao estúdio, censura o roteiro ou corta verbas.
* *Rebeldes:* Cortam a energia do estúdio momentaneamente (forçando um minigame no gerador de emergência).


* **O Destino desse Caminho:** A nave decola sob caos total na cidade. O jogador pode usar os segredos acumulados das duas partes para chantagem e garantir seu bilhete, ou ser abandonado quando a multidão invadir a rádio.

---

## ✉️ 5. Fontes de Mensagens e Exemplo Prático

### 5.1. Origem dos E-mails

1. **Governo:** Ordens oficiais, pedidos de contenção de pânico e propaganda da colônia lunar/martiana.
2. **Jornalistas Independente/Rebeldes:** Denúncias de desvio de verbas, relatórios de colapso de infraestrutura.
3. **Cartas de Ouvintes:** Histórias pessoais, apelos dramáticos de famílias desesperadas.
4. **Pedidos de Música:** Momentos de calmaria, insanidade ou codificações escondidas entre os ouvintes.

### 5.2. Exemplo de Notícia (Roteiro em Ação)

> **Fonte (Governo):** *"Informamos que os reservatórios de água do Setor Sul foram contaminados por produtos químicos durante o tumulto de ontem. NÃO há previsão de normalização e o estoque restante dura 24 horas. Mantenham a população informada para evitar o consumo."*

* **Opção 1: Falar a Verdade**
* *Impacto:* Pânico dispara (+Irritação), saques aos mercados de água, perda de confiança do Governo.


* **Opção 2: Omitir / Não Falar**
* *Impacto:* População do Setor Sul se envenena silenciosamente. A audiência cai por omissão de utilidade pública.


* **Opção 3: Mentir**
* *Impacto:* *"Manutenção de rotina agendada no Setor Sul"*. (+Esperança), o Governo aprova a conduta, mas os atingidos descobrem a verdade no dia seguinte, minando a Confiabilidade com os rebeldes.



---

## 💀 6. Condições de Derrota (Game Over)

O Game Over reflete a melancolia e o isolamento do protagonista:

### 1. Perda de Confiança Total do Governo

* Se o jogador minar os interesses do governo até o limite, o regime não perde tempo julgando-o.
* **Punição:** O governo corta a energia da rádio, tranca o perímetro externo e deixa uma caixa com suprimentos básicos na porta. O radialista é deixado para morrer ali sozinho no escuro, privado do seu único refúgio: o trabalho.

### 2. Queda Total de Audiência

* Independente do viés, se a audiência chegar a zero, a emissora é considerada inútil. O sinal é bloqueado e o jogador é isolado do mundo exterior antes do fim dos 7 dias.

---

## 🎬 7. Estrutura dos 7 Dias e Finais

| Dia | Evento Principal | Foco Narrativo |
| --- | --- | --- |
| **Dia 1** | O Anúncio do Fim | Aceitação inicial e contagem regressiva |
| **Dia 2** | Crise de Suprimentos | Racionamento de água/comida |
| **Dia 3** | Revelação da *Seleção Natural* | Povo descobre a fuga da elite para Alpha Centauri |
| **Dia 4** | O Ponto de Virada | Surgimento da resistência armada vs. Censura Total |
| **Dia 5** | Sabotagens / Visitas à Porta | Eventos interativos diretos no estúdio |
| **Dia 6** | Caos Pré-Lançamento | A batalha final pelas narrativas na cidade |
| **Dia 7** | O Fim da Terra | A decolagem da nave e o impacto final na Terra |

### Finais Possíveis

1. **O Voo da Elite (Caminho da Esperança):** O radialista é recompensado por conter a população e ganha um assento na nave *Seleção Natural*.
2. **A Revolução dos Esquecidos (Caminho da Irritação):** A população destrói os centros de lançamento. Ninguém escapa da Terra.
3. **O Chantageador do Fim (Caminho da Tensão Volátil):** O jogador chantageia ambas as facções no último segundo e garante seu lugar na nave no meio do colapso.
4. **Abandonado na Estação:** O jogador fica no estúdio até os últimos minutos enquanto a nave decola no horizonte e a atmosfera da Terra se extingue.
