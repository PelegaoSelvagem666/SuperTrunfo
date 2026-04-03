using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections; 

public class MenuSelecaoDeck : MonoBehaviour
{
    [Header("Configurações")]
    public string nomeCenaDeckBuilder = "EditorDeck";
    public string nomeCenaMenu = "MenuPrincipal";
    public string nomeCenaBatalha = "CampoBatalha"; 
    public int limiteDecks = 20;

    [Header("Interface Principal")]
    public Transform painelGradeDecks;   
    public GameObject prefabBotaoDeck;   
    public GameObject botaoCriarNovo;    
    
    [Header("Rodapé de Ações")]
    public GameObject painelAcoesContexto; 
    public TextMeshProUGUI txtDeckSelecionadoRodape;
    private int idDeckSelecionado = -1;
    private string nomeDeckSelecionado = "";
    private Transform transformDeckSelecionado;

    // --- VARIÁVEIS PARA O CLIQUE DUPLO ---
    private float tempoUltimoClique = 0f;
    private int idUltimoClique = -1;

    private List<CardData> acervoCompleto;

    void Start()
    {
        acervoCompleto = new List<CardData>(Resources.LoadAll<CardData>("Cartas"));
        AtualizarTela();
    }

    public void AtualizarTela()
    {
        transformDeckSelecionado = null;

        foreach (Transform filho in painelGradeDecks)
        {
            if (filho.gameObject != botaoCriarNovo) Destroy(filho.gameObject);
        }

        for (int i = 1; i <= limiteDecks; i++)
        {
            string chaveNome = "NomeDeck_" + i;
            if (PlayerPrefs.HasKey(chaveNome)) 
            {
                CriarBotaoVisual(i, PlayerPrefs.GetString(chaveNome));
            }
        }

        if (painelAcoesContexto != null) painelAcoesContexto.SetActive(false); 
    }

    private void CriarBotaoVisual(int id, string nomeDoDeck)
    {
        GameObject novoBotao = Instantiate(prefabBotaoDeck, painelGradeDecks);
        
        Transform objTexto = novoBotao.transform.Find("TextoNome");
        if (objTexto != null) objTexto.GetComponent<TextMeshProUGUI>().text = nomeDoDeck;

        Transform objInput = novoBotao.transform.Find("InputRenomear");
        if (objInput != null)
        {
            TMP_InputField inputField = objInput.GetComponent<TMP_InputField>();
            inputField.characterLimit = 15;
            inputField.gameObject.SetActive(false); 
            
            inputField.onEndEdit.AddListener((novoNome) => FinalizarEdicaoInline(id, novoNome, novoBotao.transform));
        }

        Transform objImagem = novoBotao.transform.Find("ImagemCapa");
        if (objImagem != null)
        {
            Image imgCapa = objImagem.GetComponent<Image>();
            string chavedoDeck = "MeuDeckSalvo_" + id;
            string deckTexto = PlayerPrefs.GetString(chavedoDeck, "");

            if (!string.IsNullOrEmpty(deckTexto))
            {
                string primeiraCarta = deckTexto.Split(',')[0];
                CardData cartaCapa = acervoCompleto.Find(c => c.name == primeiraCarta);
                if (cartaCapa != null && imgCapa != null) { imgCapa.sprite = cartaCapa.arteCarta; imgCapa.color = Color.white; }
            }
        }

        Transform objMoldura = novoBotao.transform.Find("MolduraSelecao");
        if (objMoldura != null) objMoldura.gameObject.SetActive(id == idDeckSelecionado); 
        if (id == idDeckSelecionado) transformDeckSelecionado = novoBotao.transform;

        Button btn = novoBotao.GetComponent<Button>();
        if (btn != null) btn.onClick.AddListener(() => SelecionarDeckParaAcoes(id, nomeDoDeck, novoBotao.transform));
    }

    public void SelecionarDeckParaAcoes(int id, string nome, Transform btnTransform)
    {
        // --- VERIFICAÇÃO DO CLIQUE DUPLO ---
      if (id == idUltimoClique && Time.time - tempoUltimoClique <= 0.3f) 
        { 
            idDeckSelecionado = id; // Garante que o ID do clique duplo foi registrado!
            AcaoEditarDeck(); 
            return; 
        }
        idUltimoClique = id; tempoUltimoClique = Time.time;

        idDeckSelecionado = id;
        nomeDeckSelecionado = nome;
        transformDeckSelecionado = btnTransform;
        
        if (painelAcoesContexto != null) painelAcoesContexto.SetActive(true);

        // --- A MUDANÇA VISUAL DA ESTRELINHA AQUI ---
        string deckPrincipal = PlayerPrefs.GetString("DeckPrincipalID", "");
        string textoExtra = (id.ToString() == deckPrincipal) ? " <color=yellow>[★ ATIVO]</color>" : "";
        
        if (txtDeckSelecionadoRodape != null) 
            txtDeckSelecionadoRodape.text = "Selecionado: " + nome + textoExtra;

        foreach (Transform filho in painelGradeDecks)
        {
            if (filho.gameObject != botaoCriarNovo)
            {
                Transform moldura = filho.Find("MolduraSelecao");
                if (moldura != null) moldura.gameObject.SetActive(filho == btnTransform);
            }
        }
    }

    public void CriarNovoDeck()
    {
        for (int i = 1; i <= limiteDecks; i++)
        {
            if (!PlayerPrefs.HasKey("NomeDeck_" + i))
            {
                string nomePadrao = "Novo Deck " + i;
                PlayerPrefs.SetString("NomeDeck_" + i, nomePadrao);
                PlayerPrefs.SetString("MeuDeckSalvo_" + i, ""); 
                PlayerPrefs.Save();
                
                AtualizarTela();
                
                Transform novoBtnEncontrado = null;
                foreach (Transform filho in painelGradeDecks)
                {
                    if (filho.gameObject != botaoCriarNovo)
                    {
                        Transform objTexto = filho.Find("TextoNome");
                        if (objTexto != null && objTexto.GetComponent<TextMeshProUGUI>().text == nomePadrao) 
                            novoBtnEncontrado = filho;
                    }
                }

                if (novoBtnEncontrado != null)
                {
                    SelecionarDeckParaAcoes(i, nomePadrao, novoBtnEncontrado);
                    AcaoRenomearDeck();
                }
                return;
            }
        }
    }

    public void AcaoRenomearDeck() 
    { 
        StartCoroutine(RotinaFocarInput());
    }

    private IEnumerator RotinaFocarInput()
    {
        yield return new WaitForEndOfFrame();

        if (transformDeckSelecionado != null)
        {
            Transform objTexto = transformDeckSelecionado.Find("TextoNome");
            Transform objInput = transformDeckSelecionado.Find("InputRenomear");

            if (objTexto != null && objInput != null)
            {
                TMP_InputField inputField = objInput.GetComponent<TMP_InputField>();
                inputField.text = nomeDeckSelecionado;
                objTexto.gameObject.SetActive(false);
                objInput.gameObject.SetActive(true);
                inputField.Select(); 
                inputField.ActivateInputField(); 
            }
        }
    }

    private void FinalizarEdicaoInline(int id, string novoNome, Transform btnTransform)
    {
        Transform objTexto = btnTransform.Find("TextoNome");
        Transform objInput = btnTransform.Find("InputRenomear");

        if (objInput != null) objInput.gameObject.SetActive(false);
        if (objTexto != null) objTexto.gameObject.SetActive(true);

        if (!string.IsNullOrEmpty(novoNome))
        {
            PlayerPrefs.SetString("NomeDeck_" + id, novoNome);
            PlayerPrefs.Save();
            nomeDeckSelecionado = novoNome;
            if (objTexto != null) objTexto.GetComponent<TextMeshProUGUI>().text = novoNome;
        }
    }

    public void AcaoUsarEmBatalha() 
    { 
        string deckTexto = PlayerPrefs.GetString("MeuDeckSalvo_" + idDeckSelecionado, "");
        int quantidadeCartas = string.IsNullOrEmpty(deckTexto) ? 0 : deckTexto.Split(',').Length;

        if (quantidadeCartas < 25)
        {
            Debug.LogWarning($"O deck '{nomeDeckSelecionado}' tem apenas {quantidadeCartas}/25 cartas. Termine de montá-lo primeiro!");
            return; 
        }

        PlayerPrefs.SetString("DeckPrincipalID", idDeckSelecionado.ToString());
        PlayerPrefs.Save();

        if (GerenciadorDeDeck.instancia != null) 
            GerenciadorDeDeck.instancia.deckIdAtual = idDeckSelecionado.ToString(); 

        if (txtDeckSelecionadoRodape != null) 
            txtDeckSelecionadoRodape.text = $"Selecionado: {nomeDeckSelecionado} <color=yellow>[★ ATIVO]</color>";
            
        Debug.Log($"Sucesso! O deck '{nomeDeckSelecionado}' agora é o seu deck principal para batalhas!");
    }

    public void AcaoEditarDeck() 
    { 
        // Se a mochila não existir, nós criamos uma na marra!
        if (GerenciadorDeDeck.instancia == null) 
        {
            GameObject novaMochila = new GameObject("GerenciadorDeDeck");
            novaMochila.AddComponent<GerenciadorDeDeck>();
        }

        GerenciadorDeDeck.instancia.deckIdAtual = idDeckSelecionado.ToString(); 
        SceneManager.LoadScene(nomeCenaDeckBuilder); 
    }
    public void AcaoDeletarDeck() { PlayerPrefs.DeleteKey("NomeDeck_" + idDeckSelecionado); PlayerPrefs.DeleteKey("MeuDeckSalvo_" + idDeckSelecionado); PlayerPrefs.Save(); AtualizarTela(); }
    public void VoltarAoMenu() { SceneManager.LoadScene(nomeCenaMenu); }
}