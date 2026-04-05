using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using Mirror;

public class GerenciadorMoeda : NetworkBehaviour 
{
    [Header("Interface")]
    public TextMeshProUGUI textoAviso;
    public TextMeshProUGUI textoCronometro;
    public GameObject painelBotoes;
    
    [Header("Cena Seguinte")]
    public string nomeCenaBatalha = "CampoBatalha";

    [HideInInspector] public bool escolhaFeitaPelaRede = false;
    [HideInInspector] public bool vencedorQuerComecar = false;

    private bool fezEscolhaOffline = false;
    private bool querComecarOffline = false;

void Start()
    {
        painelBotoes.SetActive(false);
        textoCronometro.gameObject.SetActive(false);

        // A REGRA DE AUTORIDADE: Se o NetworkServer está ativo, mas o NetworkClient NÃO ESTÁ,
        // significa que estamos rodando no Terminal do Linux (Servidor Headless).
        bool ehServidorDedicado = NetworkServer.active && !NetworkClient.active;
        
        string modo = PlayerPrefs.GetString("ModoJogo", "Bot");

        // Se for modo Bot E NÃO for o servidor no terminal, joga offline!
        if (modo == "Bot" && !ehServidorDedicado)
        {
            StartCoroutine(RotinaMoedaOffline());
        }
        else
        {
            if (NetworkManager.singleton != null && NetworkManager.singleton.isNetworkActive) 
            {
               textoAviso.text = "Sorteando Cara ou Coroa...";
               
               // CORREÇÃO: O nome correto da função que criamos para a moeda online!
               if (isServer) StartCoroutine(AguardarSorteioMoedaOnline());
            }
        }
    }

    // ==========================================
    // LÓGICA ONLINE (MIRROR) - AGORA BASEADA NO NET ID!
    // ==========================================

    [Server]

    private IEnumerator AguardarSorteioMoedaOnline()
    {
        // FREIO INTELIGENTE: O Servidor não usa mais tempo fixo.
        // Ele trava o código aqui num loop infinito até que as 2 conexões estejam "isReady" 
        // (que significa que eles terminaram de carregar a cena e spawnaram o boneco na rede).
        while (NetworkServer.connections.Count < 2 || !NetworkServer.connections.Values.All(c => c.isReady && c.identity != null))
        {
            yield return null;
        }

        // Dá só mais um segundinho de respiro para a interface dos clientes aparecer na tela
        yield return new WaitForSeconds(1.0f);

        var conexoes = NetworkServer.connections.Values.ToList();

        // 2. Sorteia o vencedor e pega o "ID" exclusivo dele
        int indexVencedor = Random.Range(0, 2); 
        uint vencedorNetId = conexoes[indexVencedor].identity.netId;

        // 3. Grita na rede o ID de quem venceu
        RpcMostrarResultadoParaJogadores(vencedorNetId);

        escolhaFeitaPelaRede = false;
        float timeout = 10f; // Aumentei o tempo limite para 10s por garantia
        while(timeout > 0 && !escolhaFeitaPelaRede)
        {
            timeout -= Time.deltaTime;
            yield return null;
        }

        if (!escolhaFeitaPelaRede) vencedorQuerComecar = true;

        // 4. Descobre o ID de quem vai começar a partida com base na escolha
        uint quemComecaNetId = vencedorQuerComecar ? vencedorNetId : conexoes[1 - indexVencedor].identity.netId;

        RpcDefinirQuemComecaNaArena(quemComecaNetId);

        yield return new WaitForSeconds(1.5f);
        NetworkManager.singleton.ServerChangeScene(nomeCenaBatalha);
    }

    [ClientRpc]
    private void RpcAtualizarTextoSimples(string mensagem)
    {
        if (textoAviso != null) textoAviso.text = mensagem;
    }

    [ClientRpc]
    private void RpcMostrarResultadoParaJogadores(uint vencedorNetId) 
    {
        // O cliente só precisa perguntar: "O ID que o servidor enviou é igual ao meu?"
        bool souVencedor = (NetworkClient.localPlayer != null && NetworkClient.localPlayer.netId == vencedorNetId);

        if (souVencedor)
        {
            textoAviso.text = "Você ganhou a moeda!\nQuer jogar primeiro?";
            painelBotoes.SetActive(true);
            textoCronometro.gameObject.SetActive(true);
            StartCoroutine(CronometroVisual()); 
        }
        else
        {
            textoAviso.text = "O oponente ganhou a moeda!\nAguardando a escolha dele...";
        }
    }

    [ClientRpc]
    private void RpcDefinirQuemComecaNaArena(uint quemComecaNetId)
    {
        // "O ID de quem começa a partida é o meu?"
        bool euComeco = (NetworkClient.localPlayer != null && NetworkClient.localPlayer.netId == quemComecaNetId);

        PlayerPrefs.SetInt("JogadorComeca", euComeco ? 1 : 0);
            
        textoAviso.text = "Decisão feita! Preparando a arena...";
        painelBotoes.SetActive(false);
        textoCronometro.gameObject.SetActive(false);
    }

    private IEnumerator CronometroVisual()
    {
        float tempo = 5f;
        while(tempo > 0)
        {
            if (textoCronometro != null) textoCronometro.text = Mathf.Ceil(tempo).ToString();
            tempo -= Time.deltaTime;
            yield return null;
        }
    }

    // ==========================================
    // BOTÕES DA TELA 
    // ==========================================
    public void BotaoSim() 
    { 
        if (PlayerPrefs.GetString("ModoJogo", "Bot") == "Bot") 
        { querComecarOffline = true; fezEscolhaOffline = true; }
        else 
        { 
            painelBotoes.SetActive(false); textoCronometro.gameObject.SetActive(false);
            if(NetworkClient.localPlayer != null)
                NetworkClient.localPlayer.GetComponent<JogadorRede>().CmdEnviarEscolhaMoeda(true); 
        }
    }

    public void BotaoNao() 
    { 
        if (PlayerPrefs.GetString("ModoJogo", "Bot") == "Bot") 
        { querComecarOffline = false; fezEscolhaOffline = true; }
        else 
        { 
            painelBotoes.SetActive(false); textoCronometro.gameObject.SetActive(false);
             if(NetworkClient.localPlayer != null)
                NetworkClient.localPlayer.GetComponent<JogadorRede>().CmdEnviarEscolhaMoeda(false); 
        }
    }

    // ==========================================
    // LÓGICA OFFLINE (CONTRA O BOT)
    // ==========================================
    private IEnumerator RotinaMoedaOffline()
    {
        textoAviso.text = "Sorteando Cara ou Coroa...";
        yield return new WaitForSeconds(2f);

        if (Random.Range(0, 2) == 0) 
        {
            textoAviso.text = "Você ganhou a moeda!\nQuer jogar primeiro?";
            painelBotoes.SetActive(true);
            textoCronometro.gameObject.SetActive(true);
            StartCoroutine(CronometroVisual());

            float tempoRestante = 5f;
            while (tempoRestante > 0 && !fezEscolhaOffline)
            {
                tempoRestante -= Time.deltaTime;
                yield return null;
            }

            painelBotoes.SetActive(false);
            textoCronometro.gameObject.SetActive(false);
            if (!fezEscolhaOffline) querComecarOffline = true; 

            PlayerPrefs.SetInt("JogadorComeca", querComecarOffline ? 1 : 0);
        }
        else 
        {
            textoAviso.text = "O bot ganhou e escolheu começar.";
            PlayerPrefs.SetInt("JogadorComeca", 0);
            yield return new WaitForSeconds(3f);
        }

        UnityEngine.SceneManagement.SceneManager.LoadScene(nomeCenaBatalha);
    }
}