using UnityEngine;

[CreateAssetMenu(fileName = "Nova Habilidade Serjao", menuName = "SuperTrunfo/Habilidades/Quebra Regra (Serjao)")]
public class HabilidadeQuebraRegra : HabilidadeBase
{
    public override void AtivarHabilidade(CardDisplay usuario, CardDisplay alvo)
    {
        GameManager gm = Object.FindFirstObjectByType<GameManager>();

        if (gm != null)
        {
            // 1. Se o Serjão é seu, e VOCÊ JÁ ATACOU (turnoDoJogador é true), não faz nada!
            if (usuario.pertenceAoJogador && gm.turnoDoJogador)
            {
                Debug.Log($"🛡️ {usuario.cardData.nomeCarta} atacou normalmente. Não precisa roubar a iniciativa.");
                return; // Para o código aqui, o duelo segue normal!
            }

            // 2. Se o Serjão é seu, e a IA TINHA ATACADO (turnoDoJogador é false), ROUBA A VEZ!
            if (usuario.pertenceAoJogador && !gm.turnoDoJogador)
            {
                Debug.Log($"Ativando habilidade: {usuario.cardData.tituloHabilidade} - AQUI TEM CORAGEM!");
                gm.ForcarVezDeEscolha(true); 
            }
        }
        else
        {
            Debug.LogError("GameManager não encontrado na cena!");
        }
    }
}