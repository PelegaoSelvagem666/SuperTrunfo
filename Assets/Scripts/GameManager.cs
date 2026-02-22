using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instancia;

    [Header("Configurações do Baralho")]
    public List<CardData> baralhoCompleto;
    public GameObject cartaPrefab;

    [Header("Áreas do Tabuleiro")]
    public Canvas canvasPrincipal;
    public Transform maoJogador;
    public Transform maoAdversario;
    public Transform cemiterioJogador;
    public Transform cemiterioOponente;

    [Header("Interface de Batalha")]
    public GameObject painelEscolhaAtributo; 
    public CardDisplay painelCartaDetalhe; 
    public TextMeshProUGUI textoResultado;
    public TextMeshProUGUI textoAvisoIA;

    private int pontosJogador = 0;
    private int pontosOponente = 0;

    // MODIFICADO: Variáveis de estado públicas para integração de segurança
    public bool turnoDoJogador = true; 
    public CardDisplay cartaDoJogadorNaArena; 
    public bool jogoPausado = false; 

    private CardDisplay cartaAtacanteIA; 
    private string atributoAtaqueIA;

    void Awake()
    {
        instancia = this;
    }

    void Start()
    {
        if (painelEscolhaAtributo != null) painelEscolhaAtributo.SetActive(false);
        if (painelCartaDetalhe != null) painelCartaDetalhe.gameObject.SetActive(false); 

        EmbaralharBaralho();
        ComprarCartasIniciais();
        Debug.Log("🎮 A PARTIDA COMEÇOU! É o seu turno de atacar.");
    }

    public void EmbaralharBaralho()
    {
        for (int i = 0; i < baralhoCompleto.Count; i++)
        {
            CardData temp = baralhoCompleto[i];
            int randomIndex = Random.Range(i, baralhoCompleto.Count);
            baralhoCompleto[i] = baralhoCompleto[randomIndex];
            baralhoCompleto[randomIndex] = temp;
        }
    }

    public void ComprarCartasIniciais()
    {
        for (int i = 0; i < 5; i++) 
        {
            GameObject novaCarta = Instantiate(cartaPrefab, maoJogador);
            CardDisplay display = novaCarta.GetComponent<CardDisplay>();
            display.cardData = baralhoCompleto[i];
            display.pertenceAoJogador = true; 
            display.AtualizarCarta(); 
        }

        for (int i = 5; i < 10; i++) 
        {
            GameObject novaCarta = Instantiate(cartaPrefab, maoAdversario);
            CardDisplay display = novaCarta.GetComponent<CardDisplay>();
            display.cardData = baralhoCompleto[i];
            display.pertenceAoJogador = false; 
            display.AtualizarCarta(); 
        }
    }

// NOVO: Função exclusiva para o clique (apenas olhar a carta)
 public void InspecionarCarta(CardData dados)
    {
        if (jogoPausado) return;
        
        if (painelCartaDetalhe == null) 
        {
            Debug.LogError("❌ ALERTA: O Painel Carta Detalhe não está conectado na Mesa!");
            return;
        }

        painelCartaDetalhe.gameObject.SetActive(true);
        painelCartaDetalhe.cardData = dados;
        painelCartaDetalhe.AtualizarCarta();
    }

    // ATUALIZADO: Função exclusiva para quando soltar a carta na mesa
    public void ReceberCartaNaArena(CardDisplay cartaDoJogador) 
    {
        if (jogoPausado == true) return;

        cartaDoJogadorNaArena = cartaDoJogador; 

        // Esconde a carta gigante da lateral, pois a batalha vai começar
        if (painelCartaDetalhe != null) painelCartaDetalhe.gameObject.SetActive(false);

        if (turnoDoJogador == true)
        {
            painelEscolhaAtributo.transform.SetAsLastSibling(); 
            painelEscolhaAtributo.SetActive(true);
        }
        else
        {
            ResolverDuelo(cartaDoJogadorNaArena, cartaAtacanteIA, atributoAtaqueIA);
        }
    }

    public void EscolherForca() { IniciarBatalha("Força"); }
    public void EscolherMagia() { IniciarBatalha("Magia"); }
    public void EscolherAgilidade() { IniciarBatalha("Agilidade"); }
    public void EscolherInteligencia() { IniciarBatalha("Inteligência"); }

private void IniciarBatalha(string atributoEscolhido)
    {
        painelEscolhaAtributo.SetActive(false);
        Debug.Log("🔥 VOCÊ ATACA COM: " + atributoEscolhido.ToUpper() + "!");

        CardDisplay cartaOponenteEscolhida = EscolherCartaDaIA(atributoEscolhido);

        if (cartaOponenteEscolhida != null)
        {
            cartaOponenteEscolhida.transform.SetParent(canvasPrincipal.transform);
            cartaOponenteEscolhida.transform.position = new Vector3((Screen.width / 2) + 250, Screen.height / 2, 0);
            cartaOponenteEscolhida.transform.localScale = new Vector3(0.65f, 0.65f, 0.65f);

            if (cartaOponenteEscolhida.imagemVerso != null)
            {
                cartaOponenteEscolhida.imagemVerso.gameObject.SetActive(false);
                // NOVO: Manda a carta carregar a arte agora que o verso sumiu!
                cartaOponenteEscolhida.AtualizarCarta(); 
            }

            Debug.Log("🛡️ O Oponente tenta defender com: " + cartaOponenteEscolhida.cardData.nomeCarta);
            ResolverDuelo(cartaDoJogadorNaArena, cartaOponenteEscolhida, atributoEscolhido);
        }
    }
    private CardDisplay EscolherCartaDaIA(string atributo)
    {
        CardDisplay melhorCarta = null;
        int maiorValor = -1;

        foreach (Transform filho in maoAdversario)
        {
            CardDisplay display = filho.GetComponent<CardDisplay>();
            if (display != null && display.cardData != null)
            {
                int valorAtual = PegarValorAtributo(display.cardData, atributo);
                if (valorAtual > maiorValor)
                {
                    maiorValor = valorAtual;
                    melhorCarta = display;
                }
            }
        }
        return melhorCarta;
    }

    public void TurnoDaIA()
    {
        jogoPausado = false; 

        if (maoAdversario.childCount == 0) return; 

        CardDisplay cartaEscolhida = null;
        string melhorAtributo = "";
        int maiorValor = -1;

        foreach (Transform filho in maoAdversario)
        {
            CardDisplay display = filho.GetComponent<CardDisplay>();
            if (display != null && display.cardData != null)
            {
                CardData d = display.cardData;
                if (d.forca > maiorValor) { maiorValor = d.forca; melhorAtributo = "Força"; cartaEscolhida = display; }
                if (d.magia > maiorValor) { maiorValor = d.magia; melhorAtributo = "Magia"; cartaEscolhida = display; }
                if (d.agilidade > maiorValor) { maiorValor = d.agilidade; melhorAtributo = "Agilidade"; cartaEscolhida = display; }
                if (d.inteligencia > maiorValor) { maiorValor = d.inteligencia; melhorAtributo = "Inteligência"; cartaEscolhida = display; }
            }
        }

     if (cartaEscolhida != null)
        {
            cartaAtacanteIA = cartaEscolhida;
            atributoAtaqueIA = melhorAtributo;
            
            cartaEscolhida.transform.SetParent(canvasPrincipal.transform);
            cartaEscolhida.transform.position = new Vector3((Screen.width / 2) + 250, Screen.height / 2, 0);
            cartaEscolhida.transform.localScale = new Vector3(0.65f, 0.65f, 0.65f);
            
            if (cartaEscolhida.imagemVerso != null) 
            {
                cartaEscolhida.imagemVerso.gameObject.SetActive(false);
                // NOVO: Carrega a arte da carta da IA atacante!
                cartaEscolhida.AtualizarCarta();
            }
            if (textoAvisoIA != null)
            {
                textoAvisoIA.text = $"O Oponente atacou com: {melhorAtributo.ToUpper()}!";
                textoAvisoIA.gameObject.SetActive(true);
            }

            Debug.Log($"⚠️ A IA jogou '{cartaEscolhida.cardData.nomeCarta}' e atacou com {melhorAtributo.ToUpper()}!");
        }
    }

    private int PegarValorAtributo(CardData carta, string atributo)
    {
        switch (atributo)
        {
            case "Força": return carta.forca;
            case "Magia": return carta.magia;
            case "Agilidade": return carta.agilidade;
            case "Inteligência": return carta.inteligencia;
            default: return 0;
        }
    }

private void ResolverDuelo(CardDisplay cartaJogador, CardDisplay cartaOponente, string atributo)
    {
        int valorJogador = PegarValorAtributo(cartaJogador.cardData, atributo);
        int valorOponente = PegarValorAtributo(cartaOponente.cardData, atributo);
        if (textoAvisoIA != null) textoAvisoIA.gameObject.SetActive(false);
        
        string mensagemDeCombate = "";

        if (valorJogador > valorOponente)
        {
            pontosJogador++;
            turnoDoJogador = true; 
            mensagemDeCombate = $"VITÓRIA!\n<size=50>{valorJogador} x {valorOponente}</size>";
        }
        else if (valorOponente > valorJogador)
        {
            pontosOponente++;
            turnoDoJogador = false; 
            mensagemDeCombate = $"DERROTA!\n<size=50>{valorJogador} x {valorOponente}</size>";
        }
        else
        {
            mensagemDeCombate = $"EMPATE!\n<size=50>Ambos com {valorJogador}</size>";
        }

        Debug.Log($"📊 PLACAR: Você {pontosJogador} x {pontosOponente} Oponente");

        // Pausa a interação e esconde o painel da lateral
        jogoPausado = true;
        if (painelCartaDetalhe != null) painelCartaDetalhe.gameObject.SetActive(false);

        // Inicia a contagem regressiva para limpar a mesa
        StartCoroutine(RotinaFimDeTurno(cartaJogador, cartaOponente, mensagemDeCombate));
    }

    // NOVA FUNÇÃO: O "Cronômetro" do Juiz
    private System.Collections.IEnumerator RotinaFimDeTurno(CardDisplay cartaJogador, CardDisplay cartaOponente, string mensagem)
    {
        // 1. Acende o letreiro na frente de tudo
        if (textoResultado != null)
        {
            textoResultado.text = mensagem;
            textoResultado.gameObject.SetActive(true);
            textoResultado.transform.SetAsLastSibling(); 
        }

    // 2. CONGELA A ARENA POR 4 SEGUNDOS 
        yield return new WaitForSeconds(4f);

        // 3. Esconde o letreiro e move as cartas para o cemitério (Substitui o Destroy)
        if (textoResultado != null) textoResultado.gameObject.SetActive(false);
        
        EnviarParaCemiterio(cartaJogador, cemiterioJogador);
        EnviarParaCemiterio(cartaOponente, cemiterioOponente);

        cartaDoJogadorNaArena = null;
        cartaAtacanteIA = null;

        // 4. Verifica se a partida acabou (Melhor de 5)
        if (pontosJogador >= 3 || pontosOponente >= 3 || maoJogador.childCount == 0 || maoAdversario.childCount == 0)
        {
            FinalizarJogo();
            yield break; // Interrompe a rotina de tempo
        }

        // 5. Se não acabou, destrava a mesa e continua o jogo
        jogoPausado = false; 
        if (turnoDoJogador == false)
        {
            TurnoDaIA(); 
        }
    }

    private void FinalizarJogo()
    {
        jogoPausado = true; // Trava a mesa 
        
        Debug.Log("=====================================");
        Debug.Log("🏁 FIM DA PARTIDA!");
        
        // Verifica o vencedor baseado em quem tem mais pontos
        if (pontosJogador > pontosOponente) 
        {
            Debug.Log("🏆 VITÓRIA! Você derrotou a IA!");
        }
        else if (pontosOponente > pontosJogador) 
        {
            Debug.Log("💀 DERROTA! A IA venceu a partida.");
        }
        else 
        {
            Debug.Log("🤝 EMPATE TÉCNICO! Vocês terminaram com a mesma pontuação.");
        }
            
        Debug.Log("=====================================");
    }
    private void EnviarParaCemiterio(CardDisplay carta, Transform cemiterio)
    {
        if (carta == null || cemiterio == null) return;
        
        // Define o cemitério como pai para agrupar a carta
        carta.transform.SetParent(cemiterio);
        
        // Zera a posição para a carta centralizar exatamente na âncora do cemitério
        carta.transform.localPosition = Vector3.zero; 
        
        // Reduz o tamanho da carta para não ocupar muito espaço na tela
        carta.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f); 
        
        // Aplica uma leve rotação aleatória para o visual de uma pilha de descarte
        float giroAleatorio = Random.Range(-15f, 15f);
        carta.transform.localRotation = Quaternion.Euler(0, 0, giroAleatorio);

        // Desliga o CanvasGroup para impedir que a carta seja inspecionada ou arrastada novamente
        CanvasGroup cg = carta.GetComponent<CanvasGroup>();
        if (cg != null) cg.blocksRaycasts = false;
    }
}