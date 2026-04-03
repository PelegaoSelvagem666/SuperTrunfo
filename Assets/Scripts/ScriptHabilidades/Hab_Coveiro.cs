using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "Hab_Coveiro", menuName = "Habilidades/Coveiro (Poder do Cemitério)")]
public class Hab_Coveiro : HabilidadeBase
{
    [Header("Configuração de Bônus")]
    [Tooltip("Quanto de bônus a carta ganha por CADA carta no cemitério?")]
    public int bonusPorCarta = 100;
    
    [TextArea(2, 3)]
    public string mensagemDeAtivacao = "Poder dos mortos!\nAtributos elevados!";

    public override IEnumerator AoEntrarEmCampoCoroutine(CardDisplay cartaUsuario)
    {
        // 1. Descobre para qual cemitério deve olhar
        Transform cemiterioAlvo = cartaUsuario.pertenceAoJogador ? 
            GameManager.instancia.cemiterioJogador : 
            GameManager.instancia.cemiterioOponente;

        // 2. Conta quantas cartas estão lá dentro
        int quantidadeDeMortos = cemiterioAlvo.childCount;

        // 3. Se tiver pelo menos 1 carta morta, a mágica acontece
        if (quantidadeDeMortos > 0)
        {
            int bonusTotal = quantidadeDeMortos * bonusPorCarta;
            
            Debug.Log($"[{cartaUsuario.cardData.nomeCarta}] sugou o poder de {quantidadeDeMortos} cartas no cemitério. Bônus ganho: +{bonusTotal}");

            // Aplica o bônus nos atributos da carta na arena
            cartaUsuario.valorTemporarioBonus += bonusTotal;
            cartaUsuario.AtualizarCarta();

            // Mostra o aviso na tela para o oponente tremer na base
            if (GameManager.instancia.textoAvisoIA != null)
            {
                GameManager.instancia.textoAvisoIA.text = mensagemDeAtivacao;
                GameManager.instancia.textoAvisoIA.gameObject.SetActive(true);
            }

            // Espera a animação/mensagem por 2 segundos antes de liberar o jogo
            yield return new WaitForSeconds(2f);
        }
    }
}