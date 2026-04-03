using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "Possessão Manual", menuName = "Habilidades/Possessão (Escolha Livre)")]
public class HabilidadePossessaoManual : HabilidadeBase
{
    // A MÁGICA DEVE FICAR AQUI (Na entrada em campo)
    public override IEnumerator AoEntrarEmCampoCoroutine(CardDisplay usuario)
    {
        GameManager gm = Object.FindFirstObjectByType<GameManager>();
        if (gm == null) yield break;

        if (gm.cemiterioJogador.childCount == 0 && gm.cemiterioOponente.childCount == 0) yield break;

        CardDisplay cartaMortaEscolhida = null;

        if (usuario.pertenceAoJogador)
        {
            Debug.Log("👻 O TEMPO PAROU! Clique no cemitério para encarnar um espírito!");
            
            gm.cartaSelecionadaPeloEfeito = null;
            gm.aguardandoSelecaoCemiterio = true;

            yield return new WaitUntil(() => gm.aguardandoSelecaoCemiterio == false && gm.cartaSelecionadaPeloEfeito != null);

            cartaMortaEscolhida = gm.cartaSelecionadaPeloEfeito;
        }
        else 
        {
            CardDisplay melhorAliada = PegarMelhorSoma(gm.cemiterioOponente);
            CardDisplay melhorInimiga = PegarMelhorSoma(gm.cemiterioJogador);
            
            int valorAliada = melhorAliada != null ? Somar(melhorAliada) : -1;
            int valorInimiga = melhorInimiga != null ? Somar(melhorInimiga) : -1;

            cartaMortaEscolhida = valorAliada >= valorInimiga ? melhorAliada : melhorInimiga;
        }

        if (cartaMortaEscolhida != null)
        {
            usuario.cardData = Instantiate(usuario.cardData);

            usuario.cardData.forca = cartaMortaEscolhida.cardData.forca;
            usuario.cardData.magia = cartaMortaEscolhida.cardData.magia;
            usuario.cardData.agilidade = cartaMortaEscolhida.cardData.agilidade;
            usuario.cardData.inteligencia = cartaMortaEscolhida.cardData.inteligencia;

            usuario.AtualizarCarta(); 
            
            Debug.Log($"👻 POSSESSÃO! {usuario.cardData.nomeCarta} absorveu os atributos de {cartaMortaEscolhida.cardData.nomeCarta}!");
        }
    }

    // GARANTIA DE SEGURANÇA (Esvaziamos o combate para não rodar duas vezes ou na hora errada!)
    public override IEnumerator AtivarHabilidadeCoroutine(CardDisplay usuario, CardDisplay alvo)
    {
        yield break;
    }

    private CardDisplay PegarMelhorSoma(Transform cemiterio)
    {
        CardDisplay melhor = null;
        int maior = -1;
        foreach (Transform filho in cemiterio)
        {
            CardDisplay c = filho.GetComponent<CardDisplay>();
            if (c != null && c.cardData != null)
            {
                int soma = Somar(c);
                if (soma > maior) { maior = soma; melhor = c; }
            }
        }
        return melhor;
    }

    private int Somar(CardDisplay c)
    {
        return c.cardData.forca + c.cardData.magia + c.cardData.agilidade + c.cardData.inteligencia;
    }
}