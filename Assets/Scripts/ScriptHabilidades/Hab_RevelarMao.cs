using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "NovaHabilidadeVisao", menuName = "Habilidades/Revelar Mão do Oponente")]
public class Hab_RevelarMao : HabilidadeBase
{
    public override IEnumerator AoEntrarEmCampoCoroutine(CardDisplay cartaUsuario)
    {
        if (cartaUsuario.pertenceAoJogador)
        {
            Debug.Log("Olho de Agamotto ativado! Revelando a mão do oponente...");
            foreach (Transform filho in GameManager.instancia.maoAdversario)
            {
                CardDisplay cartaOponente = filho.GetComponent<CardDisplay>();
                if (cartaOponente != null && cartaOponente.imagemVerso != null)
                {
                    cartaOponente.imagemVerso.gameObject.SetActive(false); 
                    cartaOponente.AtualizarCarta(); 
                }
            }

            GameManager.instancia.aguardandoConfirmacao = true;
            if (GameManager.instancia.painelBotaoConfirmar != null)
            {
                GameManager.instancia.painelBotaoConfirmar.SetActive(true);
                GameManager.instancia.painelBotaoConfirmar.transform.SetAsLastSibling(); 
            }
            if (GameManager.instancia.textoAvisoIA != null)
            {
                GameManager.instancia.textoAvisoIA.text = "<size=40>Visão Verdadeira!\nA mão do oponente foi revelada!</size>";
                GameManager.instancia.textoAvisoIA.gameObject.SetActive(true);
            }

            yield return new WaitUntil(() => GameManager.instancia.aguardandoConfirmacao == false);

            if (GameManager.instancia.painelBotaoConfirmar != null) GameManager.instancia.painelBotaoConfirmar.SetActive(false);
            if (GameManager.instancia.textoAvisoIA != null) GameManager.instancia.textoAvisoIA.gameObject.SetActive(false);

            foreach (Transform filho in GameManager.instancia.maoAdversario)
            {
                CardDisplay cartaOponente = filho.GetComponent<CardDisplay>();
                if (cartaOponente != null && cartaOponente.imagemVerso != null)
                {
                    cartaOponente.imagemVerso.gameObject.SetActive(true); 
                    cartaOponente.AtualizarCarta(); 
                }
            }
            yield return new WaitForSeconds(0.5f); 
        }
        else
        {
            // --- CÓDIGO DA IA (BOT) ---
            if (GameManager.instancia.textoAvisoIA != null)
            {
                GameManager.instancia.textoAvisoIA.text = "O Mago leu a sua mente!";
                GameManager.instancia.textoAvisoIA.gameObject.SetActive(true);
            }
            yield return new WaitForSeconds(2f); 
        }
    }
}