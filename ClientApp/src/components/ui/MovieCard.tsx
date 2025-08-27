import { useState } from "react";
import { Movie, PlaceholderMovie, ConcreteMovie } from "../../models/Movie";
import { AppComponentProps } from "../../pages/Layout";
import { Card, CardHeader, CardDescription, CardTitle, CardContent } from "./card";
import { Drawer, DrawerContent, DrawerDescription, DrawerHeader, DrawerTitle, DrawerTrigger } from "./drawer";
import { IconButton } from "./shadcn-io/icon-button";
import { BsEmojiGrin, BsEmojiSmile, BsEmojiLaughing, BsTrashFill } from "react-icons/bs";
import Spinner from "./Spinner";
import { IconType } from "react-icons";

import { Vote, fromNumber } from "../../consts/vote";

export type MovieCardProps = AppComponentProps & {
  movie: Movie;
  votesAvailable: Vote[];
  votesActive: Vote[];
  onVoteCallback: (vote: Vote) => void;
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

  return (
    <Drawer>
    <DrawerTrigger asChild={true}>
      <Card className="w-3xs m-2 mr-10  hover:bg-blue-200 hover:cursor-pointer">
        <CardHeader>
          <CardTitle><b className="text-2xl">{props.movie.movieName}</b></CardTitle>
          {true ? <CardDescription>{props.movie.genres.join(", ")}</CardDescription> :<></>}
        </CardHeader>
        <CardContent>
          <img className="justify-self-center" src={props.movie.posterUrl}></img>
        </CardContent>
      </Card>
    </DrawerTrigger>
    <DrawerContent>
      <div className="mx-auto w-full max-w-5/6 min-h-96 overflow-y-auto max-h-4/5">
         <DrawerHeader>
            <DrawerTitle>
              <p className="text-5xl mb-10"><b>{props.movie.movieName}</b></p>
              </DrawerTitle>
            <DrawerDescription className="text-xl text-justify">
              <div className="flex">
                <img className="mr-10 -mt-12" src="https://fwcdn.pl/fpo/00/33/120033/7606010_1.8.webp" onLoad={() => setIsLoading(false)}></img>
                { isLoading ? <Spinner isLoading></Spinner> :
                <div className="block">
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
                    {...props}
                    icon={BsEmojiGrin}
                    voteType={Vote.ThreePoints}
                    isFloatingRight={false}
                    color={[4, 184, 8]}
                    />
                    <VoteButton 
                    {...props}
                    icon={BsEmojiLaughing}
                    voteType={Vote.TwoPoints}
                    isFloatingRight={false}
                    color={[204, 228, 47]}
                    />
                    <VoteButton 
                    {...props}
                    icon={BsEmojiSmile}
                    voteType={Vote.OnePoint}
                    isFloatingRight={false}
                    color={[251, 191, 36]}
                    />
                    <VoteButton 
                    {...props}
                    icon={BsTrashFill}
                    voteType={Vote.Trash}
                    isFloatingRight={true}
                    color={[251, 0, 0]}
                    />
                    </div>
                  </div>
                </div>
                }
              </div>
            </DrawerDescription>
          </DrawerHeader>
      </div>
    </DrawerContent>
    </Drawer>
  );
};

type VoteButonProps = ConcreteMovieCardProps & {
  icon: IconType;
  voteType: Vote;
  isFloatingRight: boolean;
  color: [number, number, number];
};

const VoteButton: React.FC<VoteButonProps> = props => {
  const visibleClassName = props.isFloatingRight ? "float-right" : "";
  const isHidden = !props.votesAvailable.includes(props.voteType);
  const isActive = props.votesActive.includes(props.voteType);

  return  <IconButton
                      icon={props.icon}
                      active={isActive}
                      color={props.color} 
                      onClick={() => props.onVoteCallback(props.voteType)}
                      size="lg"
                      className={isHidden ? "hidden" : visibleClassName}
                    />;
}

