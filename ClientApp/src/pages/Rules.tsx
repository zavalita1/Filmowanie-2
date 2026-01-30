import React from 'react';
import { Button } from '../components/ui';
import { AppComponentProps, Layout } from './Layout';
import { useNavigate } from 'react-router';

const Rules: React.FC<AppComponentProps> = (props) => {

    
    return (
        <>
            {props.isMobile ? <div className="mb-10"></div> : <></>}
        <div className='md:w-2/5 flex flex-wrap justify-center-safe'> 
          <div className="w-full mt-10 mb-5">
              <h1 className="scroll-m-20 text-center text-4xl font-extrabold tracking-tight text-balance">
                  Reguły zgłaszania nominacji
              </h1>
          </div>
          <br />
                <article className="text-justify">1. Nominowany film nie może być wybrany spośród filmów na filmowaniu już oglądanych. <br />
              2. Nominowany film nie może być horrorem. O przynależności gatunkowej filmu decydują kategorie w serwisie Filmweb. <br />
              3. Nominowany film nie może być wybrany spośród filmów, które zostały nominowane uprzednio, za wyjątkiem filmów odrzuconych, które odbyły stosowny okres karencji. <br />
              4. Nominowany film musi występować w bazie serwisu Filmweb. <br />
              5. Nominowany film musi być dostępny w internecie albo musi zostać dostarczony przez nominącego w popularnym formacie cyfrowym najpóźniej w dniu następnego filmowania.
              Dla przykładu art-housowe kino filipińskie z lat 50 może być trudne do dostania, ale jeżeli masz plik z takim filmem, to jest to dozwolona nominacja.<br />
          </article>
          <div className="w-full mt-10 mb-5">
              <h1 className="scroll-m-20 text-center text-4xl font-extrabold tracking-tight text-balance">
                  Algorytm wyznaczania nominacji
              </h1>
          </div>
          <br />
          <article className="text-justify">
                    Głosujący są uszeregowani względem uzyskanego scoringu. Scoring jest określany jako:
                    <div className="m-5">
              <math display="block">
                  <mrow>
                      <mfrac>
                          <mrow>
                              <mrow>
                                  <mi>-n</mi>
                                  <msup>
                                      <mi>e</mi>
                                      <mi>α⌊d⌋</mi>
                                  </msup>
                              </mrow>
                          </mrow>
                          <mrow>
                              <mn>1</mn>
                              <mo>+⁢</mo>
                              <mi>ξ</mi>
                              <mi>p</mi>
                          </mrow>
                      </mfrac>
                  </mrow>
              </math>
              </div>
                    Gdzie <br />
                    n jest liczbą nominacji, jaką użytkownik otrzymał w określonym oknie czasowym, <br/>
                    d jest średnim czasem, jaki użytkownik potrzebował na nominowanie w przeszłości, liczonym w dniach, <br/>
                    p jest frekwencją na filmowaniu, <br />
                    natomiast parametry α i ξ wynoszą odpowiednio 5 oraz 1.2. <br /> <br />
              Z tak uzyskanym scoringiem, użytkownicy zostają uszeregowani rosnąco względem scoringu w ranking. W przypadku remisu, kolejność remisujących użytkowników jest losowana.
              Użytkownicy, którzy głosowali na wygrany film jako na śmiecia, zostają przesunięci w rankingu o połowę miejsc w górę. Użytkownik na pierwszym miejscu rankingu uzyskuje nominację.

                    <br /> <br />
              W przypadku wyznaczania nominacji za filmy, które zostały odrzucone jako śmiecie w losowaniu jedynie osoby, które trafnie wytypowały film jako śmiecia są uszeregowane w ranking. <br /> 
              Ponadto, użytkownicy z zerową frekwencją nie biorą udziału w uszeregowaniu, chyba że wyłącznie takie osoby głosowały na film jako na śmiecia.
          </article>
            </div>
    </>
  );
}

const wrappedRules: React.FC<AppComponentProps> = (props) => { return <Layout><Rules {...props}/></Layout>}

export default wrappedRules;