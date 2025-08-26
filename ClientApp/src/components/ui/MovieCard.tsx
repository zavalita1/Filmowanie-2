import { useState } from "react";
import { Movie, PlaceholderMovie, ConcreteMovie } from "../../models/Movie";
import { AppComponentProps } from "../../pages/Layout";
import { Card, CardHeader, CardDescription, CardTitle, CardContent } from "./card";
import { Drawer, DrawerContent, DrawerDescription, DrawerHeader, DrawerTitle, DrawerTrigger } from "./drawer";
import { IconButton } from "./shadcn-io/icon-button";
import { BsEmojiGrin, BsEmojiSmile, BsEmojiLaughing, BsTrashFill } from "react-icons/bs";
import Spinner from "./Spinner";
import { IconType } from "react-icons";
import { useVoteMutation } from "../../store/apis/Voting/votingApi";
import { toast } from "sonner";

export type MovieCardProps = AppComponentProps & {
  movie: Movie;
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

type ConcreteMovieCardProps = {
  movie: ConcreteMovie;
} & AppComponentProps;

const ConcreteMovieCard: React.FC<ConcreteMovieCardProps> = props => {
  const [isLoading, setIsLoading] = useState(true);
  const [vote, result] = useVoteMutation();

  const onVote = async (votes: number) => {
    const dto = { movieTitle: props.movie.movieName, votes, movieId: props.movie.movieId };
    const r = await vote(dto);
    if (r.error !== undefined) {
       toast.error("Coś poszło nie tak. Spróbuj ponownie.", {
          classNames: {
            description: "!text-foreground/80",
          },
          className: "text-5xl",
          richColors: true,
        });
    }
  }

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
                    icon={BsEmojiGrin}
                    index={3}
                    votes={props.movie.votes}
                    onVoteCallback={vote => onVote(vote)}
                    isFloatingRight={false}
                    color={[4, 184, 8]}
                    />
                    <VoteButton 
                    icon={BsEmojiLaughing}
                    index={2}
                    votes={props.movie.votes}
                    onVoteCallback={vote => onVote(vote)}
                    isFloatingRight={false}
                    color={[204, 228, 47]}
                    />
                    <VoteButton 
                    icon={BsEmojiSmile}
                    index={1}
                    votes={props.movie.votes}
                    onVoteCallback={vote => onVote(vote)}
                    isFloatingRight={false}
                    color={[251, 191, 36]}
                    />
                    <VoteButton 
                    icon={BsTrashFill}
                    index={-1}
                    votes={props.movie.votes}
                    onVoteCallback={vote => onVote(vote)}
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

type VoteButonProps = {
  icon: IconType;
  index: number;
  votes: number;
  onVoteCallback: (vote: number) => void;
  isFloatingRight: boolean;
  color: [number, number, number];
}

const VoteButton: React.FC<VoteButonProps> = props => {
  const visibleClassName = props.isFloatingRight ? "float-right" : "";

  return  <IconButton
                      icon={props.icon}
                      active={props.votes === props.index}
                      color={props.color} 
                      onClick={() => props.onVoteCallback(props.votes === props.index ? 0 : props.index)}
                      size="lg"
                      className={props.votes !== 0 && props.votes !== props.index ? "hidden" : visibleClassName}
                    />;
}

