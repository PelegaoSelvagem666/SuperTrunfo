using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "NovaApostaMortal", menuName = "Habilidades/Aposta Mortal (Cara ou Coroa)")]
public class Hab_ApostaMortal : HabilidadeBase
{
    public override IEnumerator AtivarHabilidadeCoroutine(CardDisplay cartaUsuario, CardDisplay cartaInimiga)
    {
        // Descobre o valor do atributo que está sendo disputado nesta rodada
        int valorAtualDoAtributo = GameManager.instancia.PegarValorAtributo(cartaUsuario.cardData, GameManager.instancia.atributoEmDisputa);

        string escolha = "";

        // Se for o turno do JOGADOR, abrimos o painel para ele escolher!
        if (cartaUsuario.pertenceAoJogador)
        {
            Debug.Log("Abrindo painel de Cara ou Coroa...");
            GameManager.instancia.apostaMoedaAtual = ""; // Limpa escolhas anteriores
            
            if (GameManager.instancia.painelCaraOuCoroa != null)
                GameManager.instancia.painelCaraOuCoroa.SetActive(true);
                GameManager.instancia.painelCaraOuCoroa.transform.SetAsLastSibling();

            // A MÁGICA: O jogo pausa completamente e fica esperando o jogador clicar num botão!
            yield return new WaitUntil(() => GameManager.instancia.apostaMoedaAtual != "");

            escolha = GameManager.instancia.apostaMoedaAtual;

            if (GameManager.instancia.painelCaraOuCoroa != null)
                GameManager.instancia.painelCaraOuCoroa.SetActive(false);
        }
        else
        {
            // Se for a IA usando a carta, ela escolhe aleatoriamente para ser justo!
            escolha = Random.value > 0.5f ? "Cara" : "Coroa";
            if (GameManager.instancia.textoAvisoIA != null)
            {
                GameManager.instancia.textoAvisoIA.text = $"O Oponente apostou em {escolha}!";
                GameManager.instancia.textoAvisoIA.gameObject.SetActive(true);
                yield return new WaitForSeconds(2f);
            }
        }

        // ---------------- JOGANDO A MOEDA ----------------
        // Sorteia um número de 0 a 100. Acima de 50 é Cara, abaixo é Coroa.
        string resultadoMoeda = Random.Range(0, 100) >= 50 ? "Cara" : "Coroa";
        
        bool jogadorVenceuAposta = (escolha == resultadoMoeda);

        // Mostra o resultado na tela
        if (GameManager.instancia.textoAvisoIA != null)
        {
            GameManager.instancia.textoAvisoIA.text = $"A moeda caiu em: {resultadoMoeda}!";
            GameManager.instancia.textoAvisoIA.gameObject.SetActive(true);
            yield return new WaitForSeconds(2f);
        }

        // ---------------- APLICANDO OS BUFFS/NERFS ----------------
        if (jogadorVenceuAposta)
        {
            Debug.Log($"Venceu a aposta! Atributo dobrou!");
            // Para dobrar, adicionamos o valor atual inteiro como bônus (Ex: tem 50, ganha +50 = 100)
            cartaUsuario.valorTemporarioBonus += valorAtualDoAtributo; 
        }
        else
        {
            Debug.Log($"Perdeu a aposta! Atributo caiu pela metade!");
            // Para cair pela metade, subtraímos metade do valor atual como um Nerf (Ex: tem 50, ganha -25 = 25)
            cartaUsuario.valorTemporarioBonus -= Mathf.RoundToInt(valorAtualDoAtributo / 2f);
        }

        cartaUsuario.AtualizarCarta(); // Atualiza o visual para o jogador ver o número mudando na hora!
        yield return new WaitForSeconds(1.5f); // Pausa dramática para ver o status alterado antes de baterem
    }
}