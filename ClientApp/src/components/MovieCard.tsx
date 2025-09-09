import { ReactNode, useEffect, useState } from "react";
import { VotableOrPlaceholderMovie, PlaceholderMovie, VoteableMovie, Movie } from "../models/Movie";
import { AppComponentProps } from "../pages/Layout";
import { Card, CardHeader, CardDescription, CardTitle, CardContent } from "../components/ui/card";
import { Drawer, DrawerContent, DrawerDescription, DrawerHeader, DrawerTitle, DrawerTrigger } from "./ui/drawer";
import { IconButton } from "./ui/shadcn-io/icon-button";
import { BsEmojiGrin, BsEmojiSmile, BsEmojiLaughing, BsTrashFill } from "react-icons/bs";
import Spinner from "./Spinner";
import { IconType } from "react-icons";

import { Vote } from "../consts/vote";
import clsx from "clsx";

type BaseCardProps = AppComponentProps & {
  cardFooter?: ReactNode
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

const voteTypeIconColorMap: {[key in Vote]: {icon: IconType, color: [number, number, number]}} = {
  [Vote.ThreePoints]: {icon: BsEmojiGrin, color: [4, 184, 8]},
  [Vote.TwoPoints]: {icon: BsEmojiLaughing, color: [204, 228, 47]},
  [Vote.OnePoint]: {icon: BsEmojiSmile, color: [251, 191, 36]},
  [Vote.Trash]: {icon: BsTrashFill, color: [251, 0, 0]},
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
    return (
      <Card className="w-3xs m-2 mr-10 justify-center bg-gray-100 dark:bg-gray-600">
        <CardHeader>
          <CardDescription className="text-2xl dark:text-amber-200 text-neutral-950 text-center"><b>{props.movie.title}</b></CardDescription>
        </CardHeader>
      </Card>
    );
  }
  
  return <NonPlaceholderMovieCard {...props} />;
}

interface IMovieCardCustomzer {
    renderVotingSection: () => ReactNode;
    getAdditionalCardClassNames: () => string[];
    getPosterElementClassName: () => string;
}

const NonPlaceholderMovieCard: React.FC<VoteableMovieCardProps | ReadOnlyMovieCardProps> = props => {
  const [isLoading, setIsLoading] = useState(true);
  useEffect(() => setIsLoading(!isLoading && !props.isMobile), [props.isMobile]);
  useEffect(() => {
    if (isLoading)
      setTimeout(() => setIsLoading(false), 1000);
  });
  const [isDrawerOpen, setIsDrawerOpen] = useState(false);

  const customizer = getMovieCardCustomizer(props);

  const cardClassName = clsx([
    "w-3xs",
    "m-2",
    "mr-10", 
    "hover:bg-emerald-100 hover:cursor-pointer dark:hover:bg-pink-950",
    props.isMobile ? '' : '',
    ...customizer.getAdditionalCardClassNames()
  ]);

  return (
    <Drawer open={isDrawerOpen} onOpenChange={setIsDrawerOpen}>
    <DrawerTrigger asChild={true}>
      <Card className={cardClassName}>
        <CardHeader>
          <CardTitle className="place-content-center justify-self-center dark:text-amber-200">
            <b className={getCartTitleTextSize()}>
            {`${props.movie.movieName} (${props.movie.createdYear})`}
            </b>
            </CardTitle>
          <CardDescription className="min-h-1 place-content-center justify-self-center dark:text-amber-200">{props.movie.genres.join(", ")}</CardDescription>
        </CardHeader>
        <CardContent className="mt-auto self-center-safe">
          <img className={customizer.getPosterElementClassName()} src={props.movie.posterUrl} ></img>
        </CardContent>
        {
          props.cardFooter !== undefined ? props.cardFooter : <></>
        }
      </Card>
    </DrawerTrigger>
    <DrawerContent className=" mb-0 bg-gradient-to-tr from-emerald-100 to-sky-100 dark:from-black dark:to-pink-900">
      <div className="mx-auto w-full max-w-5/6 min-h-96 overflow-y-auto max-h-4/5">
         <DrawerHeader>
            <DrawerTitle>
              <p className="text-5xl mb-10"><b>{props.movie.movieName}</b></p>
              </DrawerTitle>
            <div className="text-xl text-justify dark:text-amber-200">
              { isLoading ? <Spinner isLoading></Spinner> : <div className="flex overflow-hidden">
                { props.isMobile ? <></> : <img className="mr-10 -mt-12" src={props.movie.bigPosterUrl} alt="https://fwcdn.pl/fpo/00/33/120033/7606010_1.8.webp" onLoad={() => setIsLoading(false)}></img> }
                <div className={props.isMobile ? "block text-sm" : "block"}>
                  <p className="mb-10">{props.movie.description}</p>
                  <div >
                    <p><b>Metraż:</b> <i>{props.movie.duration}</i></p>
                    <p><b>Gatunek:</b> {props.movie.genres.join(", ")}</p>
                    <p><b>Reżyseria:</b> {props.movie.directors.join(", ")}</p>
                    <p><b>Scenariusz:</b> {props.movie.writers.join(", ")}</p>
                    <p><b>Występują:</b> {props.movie.actors.join(", ")}</p>
                    <p className="mt-4 text-center text-blue-700 dark:text-amber-100"><a href={props.movie.filmwebUrl} target="_blank">Link do filmweba.</a></p>
                  </div>
                  { customizer.renderVotingSection()}
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
    if (isVoteable(props)) {
      const voteCallback = props.onVoteCallback;
      return {
        getPosterElementClassName: () => props.votesActive.length > 0 ? "mask-r-from-blue-200 mask-r-from-70%" : "",
        getAdditionalCardClassNames: () => [getCardBgColor(props.votesActive)],
        renderVotingSection: () => {
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
      };
    }
    else return {
      getPosterElementClassName: () => "",
      getAdditionalCardClassNames: () => [],
      renderVotingSection: () => <></>
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
  const icon = voteTypeIconColorMap[props.voteType].icon;
  const color = voteTypeIconColorMap[props.voteType].color;

  return  <IconButton
                      icon={icon}
                      active={isActive}
                      color={color} 
                      onClick={() => props.onVoteCallback(props.voteType)}
                      size="lg"
                      className={isHidden ? "hidden" : visibleClassName}
                    />;
};

function getCardBgColor(votesActive: Vote[]) {
  if (votesActive.length !== 0) {
    switch (votesActive[0]) {
      case Vote.Trash:
        return "bg-red-100 dark:bg-violet-900";
      case Vote.OnePoint:
        return "bg-emerald-50 dark:bg-pink-900";
      case Vote.TwoPoints:
        return "bg-emerald-100 dark:bg-pink-800";
      case Vote.ThreePoints:
        return "bg-emerald-200 dark:bg-pink-700";
    }
  }

  return '';
}

