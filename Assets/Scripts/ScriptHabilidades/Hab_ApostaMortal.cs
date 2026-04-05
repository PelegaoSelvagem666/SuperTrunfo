using System.Collections;
using UnityEngine;
using Mirror;

[CreateAssetMenu(fileName = "NovaApostaMortal", menuName = "Habilidades/Aposta Mortal (Cara ou Coroa)")]
public class Hab_ApostaMortal : HabilidadeBase
{
    public override IEnumerator AtivarHabilidadeCoroutine(CardDisplay cartaUsuario, CardDisplay cartaInimiga)
    {
        int valorAtual = GameManager.instancia.PegarValorAtributo(cartaUsuario.cardData, GameManager.instancia.atributoEmDisputa);
        string escolha = "";
        string resultadoMoeda = "";

        if (cartaUsuario.pertenceAoJogador)
        {
            // 1. O Atacante escolhe e joga a moeda
            GameManager.instancia.apostaMoedaAtual = "";
            GameManager.instancia.resultadoMoedaRede = "";

            if (GameManager.instancia.painelCaraOuCoroa != null) 
            {
                GameManager.instancia.painelCaraOuCoroa.SetActive(true);
                // ISSO AQUI RESOLVE O BUG DA TELA ATRÁS DAS CARTAS DA MÃO:
                GameManager.instancia.painelCaraOuCoroa.transform.SetAsLastSibling(); 
            }

            yield return new WaitUntil(() => GameManager.instancia.apostaMoedaAtual != "");
            escolha = GameManager.instancia.apostaMoedaAtual;

            if (GameManager.instancia.painelCaraOuCoroa != null) GameManager.instancia.painelCaraOuCoroa.SetActive(false);

            resultadoMoeda = Random.Range(0, 100) >= 50 ? "Cara" : "Coroa";

            // 2. Avisa a rede!
            if (PlayerPrefs.GetString("ModoJogo", "Bot") != "Bot")
            {
                NetworkClient.localPlayer.GetComponent<JogadorRede>().CmdSincronizarMoeda(escolha, resultadoMoeda);
            }
        }
        else
        {
            // SE FOR O DEFENSOR
            if (PlayerPrefs.GetString("ModoJogo", "Bot") != "Bot")
            {
                // Multiplayer: Espera o Fantasma trazer o resultado do Atacante!
                GameManager.instancia.resultadoMoedaRede = "";
                yield return new WaitUntil(() => GameManager.instancia.resultadoMoedaRede != "");
                
                escolha = GameManager.instancia.apostaMoedaAtual;
                resultadoMoeda = GameManager.instancia.resultadoMoedaRede;
            }
            else
            {
                // Offline (Bot): Joga sozinho
                escolha = Random.value > 0.5f ? "Cara" : "Coroa";
                resultadoMoeda = Random.Range(0, 100) >= 50 ? "Cara" : "Coroa";
            }
        }

        // --- A PARTIR DAQUI, AS DUAS TELAS MOSTRAM A MESMA COISA ---
        if (GameManager.instancia.textoAvisoIA != null)
        {
            GameManager.instancia.textoAvisoIA.text = $"Apostou em: {escolha}!\nA moeda caiu em: {resultadoMoeda}!";
            GameManager.instancia.textoAvisoIA.gameObject.SetActive(true);
            yield return new WaitForSeconds(2.5f);
        }

        if (escolha == resultadoMoeda)
        {
            Debug.Log($"Venceu a aposta! Atributo dobrou!");
            cartaUsuario.valorTemporarioBonus += valorAtual; 
        }
        else
        {
            Debug.Log($"Perdeu a aposta! Atributo caiu pela metade!");
            cartaUsuario.valorTemporarioBonus -= Mathf.RoundToInt(valorAtual / 2f);
        }

        cartaUsuario.AtualizarCarta(); 
        yield return new WaitForSeconds(1.5f); 
    }
}