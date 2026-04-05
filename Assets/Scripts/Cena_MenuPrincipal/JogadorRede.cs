using UnityEngine;
using Mirror;

public class JogadorRede : NetworkBehaviour
{
    [SyncVar(hook = nameof(AoMudarNick))]
    public string nickNaRede = "Desconhecido";

    // 1. A variável mágica que viaja pela rede automaticamente
    [SyncVar(hook = nameof(AoReceberDeck))]
    public string deckSincronizado = "";

    // 2. Quando o SEU boneco nasce na SUA tela, ele roda isso:
public override void OnStartLocalPlayer()
    {
        // Puxa e envia APENAS o Nick. O Deck será enviado pelo GameManager!
        string meuNick = PlayerPrefs.GetString("NickJogador", "Pelego");
        CmdEnviarNickParaServidor(meuNick);
    }

    [Command]
    public void CmdEnviarNickParaServidor(string nomeDigitado)
    {
        nickNaRede = nomeDigitado;
    }

    [Command]
    public void CmdEnviarDeckParaServidor(string deckString)
    {
        deckSincronizado = deckString; // O servidor atualiza isso e manda pra todo mundo
    }

    private void AoMudarNick(string nickAntigo, string nickNovo)
    {
        gameObject.name = "Player_" + nickNovo;
    }

    // 3. O ALARME DO CORREIO (Dispara quando o servidor avisa que o deck chegou)
    private void AoReceberDeck(string deckAntigo, string deckNovo)
    {
        // Se esse fantasma NÃO sou eu (isLocalPlayer == false), é o oponente!
        // E se o deck não estiver vazio, nós pegamos o pacote!
        if (!isLocalPlayer && !string.IsNullOrEmpty(deckNovo))
        {
            Debug.Log("<color=green>[REDE]</color> Deck do oponente interceptado!");
            
            // Entrega o pacote nas mãos do GameManager
            if (GameManager.instancia != null)
            {
                GameManager.instancia.ReceberDeckDoOponente(deckNovo, nickNaRede);
            }
        }
    }
    // 4. O COMUNICADOR DA MOEDA (O Cliente avisa o Servidor o que ele clicou na tela)
    [Command]
    public void CmdEnviarEscolhaMoeda(bool querComecar)
    {
        GerenciadorMoeda moeda = Object.FindAnyObjectByType<GerenciadorMoeda>();
        if (moeda != null)
        {
            moeda.vencedorQuerComecar = querComecar;
            moeda.escolhaFeitaPelaRede = true;
        }
    }
    // ==========================================
    // SINCRONIZAÇÃO DE JOGADAS NA ARENA
    // ==========================================

    [Command]
    public void CmdJogarCarta(string nomeCarta)
    {
        RpcMostrarCartaNaMesa(nomeCarta);
    }

    [ClientRpc]
    private void RpcMostrarCartaNaMesa(string nomeCarta)
    {
        // Se a mensagem chegou e NÃO sou eu quem mandou, então foi o oponente!
        if (!isLocalPlayer && GameManager.instancia != null)
        {
            GameManager.instancia.ReceberCartaOponenteRede(nomeCarta);
        }
    }

    [Command]
    public void CmdEscolherAtributo(string atributo)
    {
        RpcDefinirAtributo(atributo);
    }

    [ClientRpc]
    private void RpcDefinirAtributo(string atributo)
    {
        if (!isLocalPlayer && GameManager.instancia != null)
        {
            GameManager.instancia.ReceberAtributoOponenteRede(atributo);
        }
    }
    [Command]
    public void CmdSincronizarMoeda(string escolha, string resultado)
    {
        RpcSincronizarMoeda(escolha, resultado);
    }

    [ClientRpc]
    private void RpcSincronizarMoeda(string escolha, string resultado)
    {
        if (!isLocalPlayer && GameManager.instancia != null)
        {
            // Entrega os dados pro GameManager do Defensor
            GameManager.instancia.apostaMoedaAtual = escolha;
            GameManager.instancia.resultadoMoedaRede = resultado;
        }
    }
}