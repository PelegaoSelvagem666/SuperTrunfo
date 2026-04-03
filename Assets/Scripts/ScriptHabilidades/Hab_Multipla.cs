using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NovaHabilidadeMultipla", menuName = "Habilidades/Habilidade Múltipla (Combo)")]
public class Hab_Multipla : HabilidadeBase
{
    [Header("Arraste várias habilidades para esta lista")]
    public List<HabilidadeBase> listaDeHabilidades;

    // Quando entrar em campo, roda todas as habilidades da lista que são "Ao Entrar Em Campo"
    public override IEnumerator AoEntrarEmCampoCoroutine(CardDisplay cartaUsuario)
    {
        foreach (HabilidadeBase hab in listaDeHabilidades)
        {
            if (hab != null)
            {
                // Pede pro GameManager rodar a corrotina da habilidade filha e espera ela terminar
                yield return GameManager.instancia.StartCoroutine(hab.AoEntrarEmCampoCoroutine(cartaUsuario));
            }
        }
    }

    // Na hora da batalha, roda todas as habilidades da lista que são "De Batalha"
    public override IEnumerator AtivarHabilidadeCoroutine(CardDisplay cartaUsuario, CardDisplay cartaInimiga)
    {
        foreach (HabilidadeBase hab in listaDeHabilidades)
        {
            if (hab != null)
            {
                yield return GameManager.instancia.StartCoroutine(hab.AtivarHabilidadeCoroutine(cartaUsuario, cartaInimiga));
            }
        }
    }
}