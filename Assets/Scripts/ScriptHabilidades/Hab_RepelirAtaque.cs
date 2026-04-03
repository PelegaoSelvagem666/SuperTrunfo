using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "NovaHabilidadeRepelir", menuName = "Habilidades/Repelir Ataque")]
public class Hab_RepelirAtaque : HabilidadeBase
{
    public override IEnumerator AtivarHabilidadeCoroutine(CardDisplay cartaUsuario, CardDisplay cartaInimiga)
    {
        // --- A TRAVA ANTI-LOOP AQUI ---
        // Se a habilidade já disparou nesta rodada, ele ignora e a porradaria acontece!
        if (GameManager.instancia.habilidadeJaUsada) 
        {
            yield break;
        }

        if (!GameManager.instancia.turnoDoJogador && GameManager.instancia.maoAdversario.childCount > 0)
        {
            Debug.Log($"{cartaUsuario.cardData.nomeCarta} usou Armadilha! A carta do oponente foi devolvida!");

            // MARCA QUE JÁ USOU: Agora ela sabe que não deve repetir com a próxima carta!
            GameManager.instancia.habilidadeJaUsada = true;
            
            GameManager.instancia.interrupcaoDeHabilidade = true;
            GameManager.instancia.ForcarTrocaDeCartaAdversario(cartaInimiga);
        }
        else
        {
            Debug.Log("Armadilha falhou: Ou você iniciou o ataque, ou era a última carta do oponente!");
        }
        
        yield return null;
    }
}