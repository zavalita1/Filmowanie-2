import { useEffect, useRef, useState } from "react";
import { useNavigate } from "react-router";
import { AppComponentProps, Layout } from "./Layout";
import { Button, Input, Label, Checkbox, Carousel, CarouselApi, CarouselContent, CarouselItem, CarouselNext, CarouselPrevious } from "../components/ui";
import { Card, CardContent } from "../components/ui/card";
import { ConfirmationDialog } from "../components/ConfirmationDialog";
import { MovieCard, ReadOnlyMovieCardProps } from "../components/MovieCard";
import { Movie } from "../models/Movie";
import { useGetMoviesThatCanBeNominatedAgainQuery, useGetPostersQuery, useNominateMutation } from "../store/apis/4-Nomination/api";

const Nomination: React.FC<AppComponentProps> = (props) => {
    const navigate = useNavigate();
    const [manualPosterPick, setManualPosterPick] = useState(false);
    const [chosenPosterUrl, setChosenPosterUrl] = useState<string | undefined>();
    const [url, setUrl] = useState("");
    const [showDialog, setShowDialog] = useState(false);
    const { data, error, isLoading } = useGetMoviesThatCanBeNominatedAgainQuery();
    const [ nominate ] = useNominateMutation();
    useEffect(() => {
        if ((props.userData?.nominations?.length ?? 0) === 0) {
            navigate('/');
        }
    }, [props.userData?.nominations]);

    if (error) {
        return <div>Coś się zjebao. Odśwież stronę.</div>
    } else if (isLoading) {
        return <div>Loading...</div>
    }
    
    let counter = 0;
    return (
        <div className="mt-20">
            <h4 className="scroll-m-20 text-center text-2xl font-extrabold tracking-tight text-balance mb-10">
                { props.userData!.nominations.length > 1 ? "Cóż za prawdziwe bogactwo nominacji. Teoretycznie powinno być to równomierniej rozdzielone, ale Cepidze nie chce się tego poprawiać. Możesz nominować filmy z lat " : "Możesz nominować film z lat "} { props.userData!.nominations.join(", ") + "."}
            </h4>
            <div className="flex w-full max-w-md gap-2 -mb-4 justify-self-center-safe">
                <Input type="email" placeholder="Wklej link do filmweba" value={url} onChange={e => setUrl(e.target.value)} onKeyDown={onKeyDown} />
                <Button type="submit" variant="outline" onClick={() => setShowDialog(true)} disabled={url.length === 0} className="bg-emerald-300 text-black hover:bg-emerald-200">
                    Давай!
                </Button>
            </div>
            <div className="justify-self-center-safe">
                <Checkbox id="toggle" disabled />
            </div>
            <Label className="justify-self-center-safe max-w-md hover:bg-accent/50 flex items-start gap-3 rounded-lg border p-3 has-[[aria-checked=true]]:border-emerald-600 has-[[aria-checked=true]]:bg-emerald-50 dark:has-[[aria-checked=true]]:border-emerald-900 dark:has-[[aria-checked=true]]:bg-emerald-950">
                <Checkbox
                    id="toggle-2"
                    defaultChecked={false}
                    checked={manualPosterPick}
                    onCheckedChange={() => setManualPosterPick(!manualPosterPick)}
                    className="data-[state=checked]:border-emerald-400 data-[state=checked]:bg-emerald-400 data-[state=checked]:text-white dark:data-[state=checked]:border-emerald-700 dark:data-[state=checked]:bg-emerald-700"
                />
                <div className="grid gap-1.5 font-normal">
                    <p className="text-sm leading-none font-medium">
                        Samodzielny wybór plakatu
                    </p>
                    <p className="text-muted-foreground text-sm">
                        Przy zaznaczonej opcji będziesz miec możliwości wyboru plakatu samodzielnie zamiast zdawac się na domyślny wybór.
                    </p>
                </div>
            </Label>
            <div>
                <ConfirmationDialog isOpen={showDialog} onClose={() => setShowDialog(false)} isLarge={!props.isMobile} onAction={() => nominateMovie()}
                        dialogActionText="Nie pierdol, tylko nominuj."
                        dialogCancelText="Dobra, zmiękła mi pałka, chcę rozważyć inny wariant..."
                        dialogContent={ manualPosterPick ? <PosterPicker movieUrl={url} isMobile={props.isMobile} onPosterPick={e => setChosenPosterUrl(e)}/> : "Czy na pewno chcesz nominować podany film? Po zaakceptowaniu nie będzie odwrotu."}
                        dialogTitle="Czy aby na pewno?"
                        dialogSubtitle="Pozostaje jeszcze wybór plakatu:"
                      />
                <div className="flex flex-row flex-wrap justify-center mt-10">
                      { data!.map(d => renderMovieCard(d, counter++))}
                </div>
            </div>
        </div>
    )

    function onKeyDown(e: React.KeyboardEvent<HTMLDivElement>) {
        if (e.key === 'Enter') {
            e.preventDefault();
            nominateMovie();
        }
    }

    function nominateMovie() {
        const dto = { filmwebUrl: url, posterUrl: chosenPosterUrl }
        setUrl("");
        nominate(dto);
    }

    function renderMovieCard(movie: Movie, key: number) {
        const cardProps = {...props, movie } satisfies ReadOnlyMovieCardProps;
        const cardFooter = <div className="self-center" onClick={(e) => {
            setUrl(movie.filmwebUrl);
            e.preventDefault();
            window.scrollTo({ top: 0});
        }
        }><Button variant="default" className="cursor-pointer hover:bg-emerald-900 bg-emerald-500">Chcę ten! Przeklej link.</Button></div>;
        return (<MovieCard {...cardProps} key={key} cardFooter={cardFooter}/>)
    }
}

type PosterPickerProps = {
    movieUrl: string;
    isMobile: boolean;
    onPosterPick: (posterUrl: string) => void;
};

const PosterPicker: React.FC<PosterPickerProps> = props => {
    const {data, error, isLoading} = useGetPostersQuery(props.movieUrl);
    const [chosenIndex, setChosenIndex] = useState<number | null>(null);
    const [api, setApi] = useState<CarouselApi>();
    const stateRef = useRef<number | null>();

    const cardsInCarouselForBigScreens = 5;
    stateRef.current = chosenIndex;

    useEffect(() => {
        if (!api) { return; }

        api.on("select", e => {
            if (props.isMobile) {
                setChosenIndexAndInvokeCallback(e.selectedScrollSnap());
                return;
            }
            
            if (stateRef.current === null || (stateRef.current! - e.selectedScrollSnap() >= 0 && stateRef.current! - e.selectedScrollSnap() < cardsInCarouselForBigScreens)) {
                return;
            }
            
            const diff = e.selectedScrollSnap() - e.previousScrollSnap();
            setChosenIndexAndInvokeCallback((stateRef.current! + diff));
        });
    }, [api])

    if (error) {
        return <div>Coś się zesrao :(</div>
    } else if (isLoading) {
        return <div>Loading...</div>
    }

    if (props.isMobile) {
        return (
            <div className="flex justify-center">
                <Carousel className="w-4/6 max-w-xs" setApi={setApi}>
                    <CarouselContent>
                        {data!.map((posterUrl, index) => (
                            <CarouselItem key={index}>
                                <div className="p-1">
                                    <Card>
                                        <CardContent className="flex aspect-square items-center justify-center">
                                            <img src={posterUrl} />
                                        </CardContent>
                                    </Card>
                                </div>
                            </CarouselItem>
                        ))}
                    </CarouselContent>
                    <CarouselPrevious className="" />
                    <CarouselNext />
                </Carousel>
            </div>
        )
    }

    return (
         <div className="flex justify-center">
        <Carousel opts={{ align: "start" }} className="w-full max-w-3xl" setApi={setApi}>
            <CarouselContent>
                {data!.map((posterUrl, index) => (
                    <CarouselItem key={index} className={`md:basis-1/3 lg:basis-1/${cardsInCarouselForBigScreens}`}>
                        <div className="p1">
                            <Card className="py-0">
                                <CardContent className={index === chosenIndex ? "flex aspect-square items-center justify-center px-0 border-emerald-300 border-4" : "flex aspect-square items-center justify-center px-0 border-4"}>
                                    <img src={posterUrl} onClick={() => setChosenIndexAndInvokeCallback(index)}/>
                                </CardContent>
                            </Card>
                        </div>
                    </CarouselItem>
                ))}
            </CarouselContent>
            <CarouselPrevious />
            <CarouselNext />
        </Carousel>
        </div>
    );

    function setChosenIndexAndInvokeCallback(index: number)  {
        setChosenIndex(index);
         props.onPosterPick(data![index]);
    }
}

const wrappedNomination: React.FC<AppComponentProps> = (props) => { return <Layout disableCenterVertically={true}><Nomination {...props} /></Layout>}

export default wrappedNomination;