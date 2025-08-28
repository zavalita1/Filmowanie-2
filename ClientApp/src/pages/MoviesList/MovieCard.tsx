import { useEffect, useState } from "react";
import { Movie, PlaceholderMovie, ConcreteMovie } from "../../models/Movie";
import { AppComponentProps } from "../Layout";
import { Card, CardHeader, CardDescription, CardTitle, CardContent } from "../../components/ui/card";
import { Drawer, DrawerContent, DrawerDescription, DrawerHeader, DrawerTitle, DrawerTrigger } from "../../components/ui/drawer";
import { IconButton } from "../../components/ui/shadcn-io/icon-button";
import { BsEmojiGrin, BsEmojiSmile, BsEmojiLaughing, BsTrashFill } from "react-icons/bs";
import Spinner from "../../components/ui/Spinner";
import { IconType } from "react-icons";

import { Vote } from "../../consts/vote";
import clsx from "clsx";

export type MovieCardProps = AppComponentProps & {
  movie: Movie;
  votesAvailable: Vote[];
  votesActive: Vote[];
  onVoteCallback: (vote: Vote) => void;
}

const voteTypeIconColorMap: {[key in Vote]: {icon: IconType, color: [number, number, number]}} = {
  [Vote.ThreePoints]: {icon: BsEmojiGrin, color: [4, 184, 8]},
  [Vote.TwoPoints]: {icon: BsEmojiLaughing, color: [204, 228, 47]},
  [Vote.OnePoint]: {icon: BsEmojiSmile, color: [251, 191, 36]},
  [Vote.Trash]: {icon: BsTrashFill, color: [251, 0, 0]},
}

export const MovieCard: React.FC<MovieCardProps> = props => {
  const placeholderMovie = props.movie as PlaceholderMovie;
  if (!!placeholderMovie?.decade) {
    let message = `${placeholderMovie.title} ${placeholderMovie.decade}`;
    message = message.slice(0, -1) + 'X';
    return (
      <Card className="w-3xs m-2 mr-10 justify-center bg-gray-100">
        <CardHeader>
          <CardDescription className="text-2xl text-neutral-950 text-center"><b>{placeholderMovie.title}</b></CardDescription>
        </CardHeader>
      </Card>
    );
  }
  
  const concreteMovieProps = {...props, movie: props.movie as ConcreteMovie};
  return <ConcreteMovieCard {...concreteMovieProps} />;
}

type ConcreteMovieCardProps = MovieCardProps & {
  movie: ConcreteMovie;
};

const ConcreteMovieCard: React.FC<ConcreteMovieCardProps> = props => {
  const [isLoading, setIsLoading] = useState(true);
  useEffect(() => setIsLoading(!isLoading && !props.isMobile), [props.isMobile]);
  const [isDrawerOpen, setIsDrawerOpen] = useState(false);
  const extendedProps = {...props, onVoteCallback: (vote: Vote) => { 
    props.onVoteCallback(vote); 
    setIsDrawerOpen(false);
  }};
  const [isVotingConfirmationDialogOpen, setIsVotingConfirmationDialogOpen] = useState(false);

  const cardClassName = clsx([
    "w-3xs",
    "m-2",
    "mr-10", 
    "hover:bg-blue-200 hover:cursor-pointer",
    props.isMobile ? '' : '',
    getCardBgColor(props.votesActive)
  ]);

  return (
    <Drawer open={isDrawerOpen} onOpenChange={setIsDrawerOpen}>
    <DrawerTrigger asChild={true}>
      <Card className={cardClassName}>
        <CardHeader>
          <CardTitle><b className="text-2xl">{props.movie.movieName}</b></CardTitle>
          {true ? <CardDescription>{props.movie.genres.join(", ")}</CardDescription> :<></>}
        </CardHeader>
        <CardContent>
          <img className={props.votesActive.length > 0 ? "mask-r-from-blue-200 mask-r-from-70%" : ""} src={props.movie.posterUrl}></img>
        </CardContent>
      </Card>
    </DrawerTrigger>
    <DrawerContent>
      <div className="mx-auto w-full max-w-5/6 min-h-96 overflow-y-auto max-h-4/5">
         <DrawerHeader>
            <DrawerTitle>
              <p className="text-5xl mb-10"><b>{props.movie.movieName}</b></p>
              </DrawerTitle>
            <div className="text-xl text-justify">
              { isLoading ? <Spinner isLoading></Spinner> : <div className="flex">
                { props.isMobile ? <></> : <img className="mr-10 -mt-12" src="https://fwcdn.pl/fpo/00/33/120033/7606010_1.8.webp" onLoad={() => setIsLoading(false)}></img> }
                <div className={props.isMobile ? "block text-sm" : "block"}>
                  <p className="mb-10">{props.movie.description}</p>
                  <div>
                    <p><b>Metraż:</b> <i>{props.movie.duration}</i></p>
                    <p><b>Gatunek:</b> {props.movie.genres.join(", ")}</p>
                    <p><b>Reżyseria:</b> {props.movie.directors.join(", ")}</p>
                    <p><b>Scenariusz:</b> {props.movie.writers.join(", ")}</p>
                    <p><b>Występują:</b> {props.movie.actors.join(", ")}</p>
                    <p className="mt-4 text-center text-blue-700"><a href={props.movie.filmwebUrl} target="_blank">Link do filmweba.</a></p>
                  </div>
                  <div className="mt-20">
                    <b>Głosowanie:</b>
                    <br/>
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
                  </div>
                </div>
              </div>
                }
            </div>
          </DrawerHeader>
      </div>
    </DrawerContent>
    </Drawer>
  );
};

type VoteButonProps = ConcreteMovieCardProps & {
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
        return "bg-red-100";
      case Vote.OnePoint:
        return "bg-amber-100";
      case Vote.TwoPoints:
        return "bg-lime-100";
      case Vote.ThreePoints:
        return "bg-lime-200";
    }
  }

  return '';
}

