import { ReactNode, useEffect, useState } from "react";
import { PlaceholderMovie, VoteableMovie, Movie } from "../models/Movie";
import { AppComponentProps } from "../pages/Layout";
import { Card, CardHeader, CardDescription, CardTitle, CardContent } from "../components/ui/card";
import { Drawer, DrawerContent, DrawerHeader, DrawerTitle, DrawerTrigger } from "./ui/drawer";
import { IconButton } from "./ui/shadcn-io/icon-button";
import { BsEmojiGrin, BsEmojiSmile, BsEmojiLaughing, BsTrashFill } from "react-icons/bs";
import Spinner from "./Spinner";
import { IconType } from "react-icons";
import { useResetNominationMutation } from '../store/apis/4-Nomination/api';

import { Vote } from "../consts/vote";
import clsx from "clsx";
import { Button } from "./ui";

type BaseCardProps = AppComponentProps & {
  cardFooter?: ReactNode;
  simplifiedView: boolean;
  useAltDescription: boolean;
}

export type ReadOnlyMovieCardProps = BaseCardProps & {
  movie: Movie;
}

export type PlaceholderMovieCardProps = BaseCardProps & {
  movie: PlaceholderMovie
  isPlaceholder: true
}

export type VoteableMovieCardProps = BaseCardProps & {
  movie: VoteableMovie;
  votesAvailable: Vote[];
  votesActive: Vote[];
  onVoteCallback: (vote: Vote) => void;
};

export type MovieCardProps = ReadOnlyMovieCardProps | VoteableMovieCardProps | PlaceholderMovieCardProps;

const voteTypeIconColorMap: {[key in Vote]: {icon: IconType, color: [number, number, number], dataTest: string }} = {
  [Vote.ThreePoints]: {icon: BsEmojiGrin, color: [37, 234, 239], dataTest: "threePointsVote"},
  [Vote.TwoPoints]: {icon: BsEmojiLaughing, color: [112, 207, 34], dataTest: "twoPointsVote"},
  [Vote.OnePoint]: {icon: BsEmojiSmile, color: [145, 118, 48], dataTest: "onePointVote"},
  [Vote.Trash]: {icon: BsTrashFill, color: [251, 0, 0], dataTest: "trashVote"},
}

function isPlaceholderCardProps(props: MovieCardProps): props is PlaceholderMovieCardProps {
  return (props as PlaceholderMovieCardProps)?.isPlaceholder !== undefined;
}
function isVoteable(props: MovieCardProps): props is VoteableMovieCardProps {
  return (props as VoteableMovieCardProps)?.votesActive !== undefined;
}

export const MovieCard: React.FC<MovieCardProps> = props => {
  if (isPlaceholderCardProps(props)) {
    let message = `${props.movie.title} ${props.movie.decade}`;
    message = message.slice(0, -1) + 'X';

    const placeholderCardClassname = clsx([
      "w-3xs",
      "m-2",
      "justify-center",
      "bg-gray-100",
      "dark:bg-gray-600",
      "h-fit",
      props.isMobile ? "" : "mr-10",
    ]);

    return (
      <Card className={placeholderCardClassname}>
        <CardHeader>
          <CardDescription className="text-2xl dark:text-amber-300 text-neutral-950 text-center"><b>{props.movie.title}</b></CardDescription>
        </CardHeader>
      </Card>
    );
  }
  
  return <NonPlaceholderMovieCard {...props} />;
}

interface IMovieCardCustomzer {
    renderVotingSection: () => ReactNode;
    getAdditionalCardClassNames: () => string[];
    getCardContents: () => ReactNode;
    overridesCardColor: () => boolean;
}

const NonPlaceholderMovieCard: React.FC<VoteableMovieCardProps | ReadOnlyMovieCardProps> = props => {
  const [isLoading, setIsLoading] = useState(true);
  const [ resetNomination ] = useResetNominationMutation();
  useEffect(() => setIsLoading(!isLoading && !props.isMobile), [props.isMobile]);
  useEffect(() => {
    if (isLoading)
      setTimeout(() => setIsLoading(false), 1000);
  });
  const [isDrawerOpen, setIsDrawerOpen] = useState(false);
  const movieDescription = props.useAltDescription && !!props.movie.altDescription ? props.movie.altDescription : props.movie.description;
  const customizer = getMovieCardCustomizer(props);

  const cardClassName = clsx([
    //"w-3xs",
    "m-2",
    "hover:bg-emerald-100 hover:cursor-pointer dark:hover:bg-pink-950",
    customizer.overridesCardColor() ? "" : "dark:bg-linear-to-tl/increasing dark:from-black dark:to-pink-900",
    customizer.overridesCardColor() ? "" : "bg-linear-to-tl/hsl from-emerald-100 to-sky-100",
    props.isMobile ? '' : 'mr-10',
    ...customizer.getAdditionalCardClassNames()
  ]);

  const drawerTitleClassName = clsx([
    "mb-10",
    "dark:text-amber-300",
    props.isMobile ? "text-2xl": "text-5xl"
  ]);

  return (
    <Drawer open={!props.simplifiedView && isDrawerOpen} onOpenChange={setIsDrawerOpen}>
    <DrawerTrigger asChild={true}>
      <Card className={cardClassName}>
        <CardHeader>
          <CardTitle className="place-content-center justify-self-center dark:text-amber-300">
            <b className={getCartTitleTextSize()}>
            {`${props.movie.movieName} (${props.movie.createdYear})`}
            </b>
            </CardTitle>
          <CardDescription className="min-h-1 place-content-center justify-self-center dark:text-amber-300">{props.movie.genres.join(", ")}</CardDescription>
        </CardHeader>
        { customizer.getCardContents() }
        {
          props.cardFooter !== undefined ? props.cardFooter : <></>
        }
      </Card>
    </DrawerTrigger>
    <DrawerContent className="mb-0 bg-gradient-to-tr from-emerald-100 to-sky-100 dark:from-black dark:to-pink-900">
      <div className="mx-auto w-full max-w-5/6 min-h-96 overflow-y-auto max-h-4/5">
         <DrawerHeader>
            <DrawerTitle>
              <p className={drawerTitleClassName}><b>{props.movie.movieName}</b></p>
              </DrawerTitle>
            <div className="text-xl text-justify dark:text-amber-300">
              { isLoading ? <Spinner isLoading></Spinner> : <div className="flex overflow-hidden">
                { props.isMobile ? <></> : <img className="mr-10 -mt-12 h-max max-w-3/5" src={props.movie.bigPosterUrl} alt="https://fwcdn.pl/fpo/00/33/120033/7606010_1.8.webp" onLoad={() => setIsLoading(false)}></img> }
                <div className={props.isMobile ? "block text-sm" : "block"}>
                  <p className="mb-10 whitespace-pre-line leading-7 [&:not(:first-child)]:mt-6">{movieDescription}</p>
                  <div >
                    <p><b>Metraż:</b> <i>{props.movie.duration}</i></p>
                    <p><b>Gatunek:</b> {props.movie.genres.join(", ")}</p>
                    <p><b>Reżyseria:</b> {props.movie.directors.join(", ")}</p>
                    <p><b>Scenariusz:</b> {props.movie.writers.join(", ")}</p>
                    <p><b>Występują:</b> {props.movie.actors.join(", ")}</p>
                    <p className="mt-4 text-center text-blue-700 dark:text-pink-300 hover:cursor-pointer"><a href={props.movie.filmwebUrl} target="_blank">Link do filmweba.</a></p>
                  </div>
                  { customizer.renderVotingSection()}
                  { !props.userData?.isAdmin ? <></> : <Button className="mt-20" onClick={() => resetNomination(props.movie.movieId)}>Delete movie</Button>} 
                </div>
              </div>
                }
            </div>
          </DrawerHeader>
      </div>
    </DrawerContent>
    </Drawer>
  );

  function getCartTitleTextSize() {
    const length = props.movie.movieName.length;
    if (length < 10) return "text-2xl";
    else if (length < 14) return "text-xl";
    else return "text-3md";
  }

  function getMovieCardCustomizer(props: VoteableMovieCardProps | ReadOnlyMovieCardProps): IMovieCardCustomzer {
    let result = {
      getCardContents: () => getStandardCardContents(props),
      getAdditionalCardClassNames: () => [] as string[],
      renderVotingSection: () => <></>,
      overridesCardColor: () => false
    };
    
    if (isVoteable(props)) {
      result.getAdditionalCardClassNames = () => [getCardBgColor(props.votesActive)];
      result.overridesCardColor = () => props.votesActive.length !== 0;
      result.renderVotingSection = () => getVotingSection(props);

      if (props.simplifiedView) {
      const posterElementClassName = props.votesActive.length > 0 ? " max-h-[285px] max-w-[200px] self-center-safe w-fit mask-r-from-blue-200 mask-r-from-70%" : " max-h-[285px] max-w-[200px]self-center-safe w-fit";
      result.getCardContents = () => (<CardContent className="h-full self-center-safe xl:w-xl flex flex-nowrap flex-col place-content-center">
          <img className={posterElementClassName} src={props.movie.posterUrl} ></img>
          <div className="mt-2">{movieDescription}</div>
          <div className="mt-auto">
            {getVotingSection(props)}
          </div>
        </CardContent>)
      }
    };

    return result;

    function getStandardCardContents(props: VoteableMovieCardProps | ReadOnlyMovieCardProps) {
      const posterElementClassName = "votesActive" in props && props.votesActive.length > 0 ? " max-h-[285px] max-w-[200px] justify-self-center mask-r-from-blue-200 mask-r-from-70%" : " max-h-[285px] max-w-[200px] justify-self-center";

      return <CardContent className="mt-auto self-center-safe">
          <img className={posterElementClassName} src={props.movie.posterUrl} ></img>
        </CardContent>;
    }

    function getVotingSection(props: VoteableMovieCardProps) {
      const voteCallback = props.onVoteCallback;
      const extendedProps = ({
        ...props, onVoteCallback: (vote: Vote) => {
          voteCallback(vote);
          setIsDrawerOpen(false);
        }
      });
      return (<div className="mt-20">
        <b>Głosowanie:</b>
        <br />
        <div>
          <VoteButton
            {...extendedProps}
            voteType={Vote.ThreePoints}
            isFloatingRight={false}
          />
          <VoteButton
            {...extendedProps}
            voteType={Vote.TwoPoints}
            isFloatingRight={false}
          />
          <VoteButton
            {...extendedProps}
            voteType={Vote.OnePoint}
            isFloatingRight={false}
          />
          <VoteButton
            {...extendedProps}
            voteType={Vote.Trash}
            isFloatingRight={true}
          />
        </div>
      </div>);
    }
  }
};

type VoteButonProps = VoteableMovieCardProps & {
  voteType: Vote;
  isFloatingRight: boolean;
};

const VoteButton: React.FC<VoteButonProps> = props => {
  const visibleClassName = props.isFloatingRight ? "float-right" : "";
  const isHidden = !props.votesAvailable.includes(props.voteType);
  const isActive = props.votesActive.includes(props.voteType);
  const {icon, color, dataTest } = voteTypeIconColorMap[props.voteType];

  return  <IconButton
                      icon={icon}
                      active={isActive}
                      color={color} 
                      onClick={() => props.onVoteCallback(props.voteType)}
                      size="lg"
                      className={isHidden ? "hidden" : visibleClassName}
                      dataTest={dataTest}
                    />;
};

function getCardBgColor(votesActive: Vote[]) {
  if (votesActive.length !== 0) {
    switch (votesActive[0]) {
      case Vote.Trash:
        return "bg-red-100 dark:bg-gray-800";
      case Vote.OnePoint:
        return "bg-emerald-200 dark:bg-violet-900";
      case Vote.TwoPoints:
        return "bg-emerald-300 dark:bg-violet-700";
      case Vote.ThreePoints:
        return "bg-emerald-400 dark:bg-violet-600";
    }
  }

  return '';
}

