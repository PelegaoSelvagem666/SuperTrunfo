using UnityEngine;
using UnityEngine.SceneManagement; 
using TMPro; 
using Mirror; 
using System.Collections;

public class MenuPrincipal : NetworkBehaviour
{
    public string nomeCenaDeckBuilder = "EditorDeck"; 
    public string nomeCenaBatalha = "CampoBatalha"; 
    public string nomeCenaSelecao = "SelecaoDeck"; 
    
    [Header("Avisos")]
    public GameObject avisoSemDeck; 

    [Header("Interface Online")]
    public TMP_InputField campoDeNick; 
    public GameObject painelBotoesIniciais; 
    public GameObject painelBotoesJogar; 

    private void Start()
    {
        Application.targetFrameRate = 30;

        if (campoDeNick != null) campoDeNick.text = PlayerPrefs.GetString("NickJogador", "");
        if (avisoSemDeck != null) avisoSemDeck.SetActive(false);

        // Se o servidor abriu essa cena Headless, a gente começa a checar os jogadores
        if (NetworkServer.active) StartCoroutine(RotinaServidorAguardandoDoisJogadores());
    }

    [Server]
    private IEnumerator RotinaServidorAguardandoDoisJogadores()
    {
        // O Servidor fica travado na tela de Menu, contando os jogadores escondido!
        while (NetworkServer.connections.Count < 2)
        {
            yield return null;
        }

        Debug.Log("[SERVIDOR] Dois jogadores entraram! Carregando a Sala da Moeda...");
        NetworkManager.singleton.ServerChangeScene("CenaMoeda");
    }

    public void AoClicarEmJogarInicial()
    {
        string nickEscolhido = string.IsNullOrEmpty(campoDeNick.text) ? "Pelego" : campoDeNick.text;
        PlayerPrefs.SetString("NickJogador", nickEscolhido);
        PlayerPrefs.Save();

        if (painelBotoesIniciais != null) painelBotoesIniciais.SetActive(false);
        if (painelBotoesJogar != null) painelBotoesJogar.SetActive(true);
    }

public void JogarContraBot()
    {
        PlayerPrefs.SetString("ModoJogo", "Bot");
        PlayerPrefs.Save();
        
        // Verifica o deck primeiro para não deixar o jogador entrar sem cartas
        string deckPrincipal = PlayerPrefs.GetString("DeckPrincipalID", "");
        if (string.IsNullOrEmpty(deckPrincipal))
        {
            if (avisoSemDeck != null) avisoSemDeck.SetActive(true); 
            return; 
        }
        if (avisoSemDeck != null) avisoSemDeck.SetActive(false);

        // A MÁGICA: Ligamos o Mirror no modo "Host Local" silenciosamente!
        // O Mirror fica feliz achando que é multiplayer e não desliga os scripts.
        NetworkManager.singleton.StartHost();
        
        // Força a mudança de cena caso o Mirror não faça isso automaticamente
        if (string.IsNullOrEmpty(NetworkManager.singleton.onlineScene))
        {
            SceneManager.LoadScene("CenaMoeda");
        }
    }

    public void CriarSala()
    {
        PlayerPrefs.SetString("ModoJogo", "Host");
        PlayerPrefs.Save();
        Debug.Log("Iniciando Servidor Dedicado...");
        NetworkManager.singleton.StartServer(); 
    }

    public void EntrarSala()
    {
        PlayerPrefs.SetString("ModoJogo", "Client");
        PlayerPrefs.Save();
        
        string ipDoServidor = "192.168.2.114"; // <-- Volte isso pro No-IP depois que testar local!

        NetworkManager.singleton.networkAddress = ipDoServidor;
        Debug.Log($"Conectando aos servidores oficiais em: {ipDoServidor}...");
        NetworkManager.singleton.StartClient();
    }

    public void IrParaBatalha()
    {
        string deckPrincipal = PlayerPrefs.GetString("DeckPrincipalID", "");
        if (string.IsNullOrEmpty(deckPrincipal))
        {
            if (avisoSemDeck != null) avisoSemDeck.SetActive(true); 
            return; 
        }

        if (avisoSemDeck != null) avisoSemDeck.SetActive(false);
        SceneManager.LoadScene("CenaMoeda");
    }

    public void IrParaDeckBuilder() { SceneManager.LoadScene(nomeCenaSelecao); }
    public void SairDoJogo() { Application.Quit(); }
    public void VoltarParaMenuInicial()
    {
        if (painelBotoesJogar != null) painelBotoesJogar.SetActive(false);
        if (painelBotoesIniciais != null) painelBotoesIniciais.SetActive(true);
    }
}