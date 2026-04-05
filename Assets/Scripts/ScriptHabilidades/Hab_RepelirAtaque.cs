using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "NovaHabilidadeRepelir", menuName = "Habilidades/Repelir Ataque")]
public class Hab_RepelirAtaque : HabilidadeBase
{
    public override IEnumerator AtivarHabilidadeCoroutine(CardDisplay cartaUsuario, CardDisplay cartaInimiga)
    {
        if (GameManager.instancia.habilidadeJaUsada) yield break;

        bool souDefensor = (cartaUsuario.pertenceAoJogador && !GameManager.instancia.turnoDoJogador) ||
                           (!cartaUsuario.pertenceAoJogador && GameManager.instancia.turnoDoJogador);

        if (!souDefensor) yield break;

        Transform maoDoAtacante = GameManager.instancia.turnoDoJogador ? GameManager.instancia.maoJogador : GameManager.instancia.maoAdversario;
        
        if (maoDoAtacante.childCount == 0)
        {
            Debug.Log($"Armadilha falhou: O atacante não tem outras cartas!");
            yield break;
        }

        GameManager.instancia.habilidadeJaUsada = true;
        GameManager.instancia.interrupcaoDeHabilidade = true; 

        Debug.Log($"🛡️ ARMADILHA! {cartaUsuario.cardData.nomeCarta} repeliu o ataque!");

        // Aciona o Rebobinador no GameManager!
        GameManager.instancia.RebobinarAtaqueRepelido(cartaInimiga);
        
        yield return new WaitForSeconds(1.5f);
    }
}