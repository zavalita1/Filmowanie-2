import { Layout, AppComponentProps } from "../Layout";
import { useGetCurrentVotingQuery } from '../../store/apis/Voting/votingApi';
import { useNavigate } from "react-router";
import React, { useEffect, useState } from "react";
import { VotingStatus } from "../../consts/votingStatus";
import { Skeleton } from "../../components/ui/skeleton";
import { MovieCard } from "./MovieCard";
import { ConcreteMovie, Movie } from "../../models/Movie";
import * as Vote from "../../consts/vote";
import { useVoteMutation } from "../../store/apis/Voting/votingApi";
import { toast } from "sonner";
import Confetti from "../../components/Confetti";

const MoviesList: React.FC<AppComponentProps> = (props) => {
  const navigate = useNavigate();
  useEffect(() => {
    if (!props.userData)
      navigate('/');

    if (props.votingStatus !== VotingStatus.Voting)
      navigate('/');

  }, [props.userData, props.votingStatus]);
  const [displayMode, setDisplayMode] = useState<'Cards' | 'Carousel'>('Cards');
  const { data, error, isLoading } = useGetCurrentVotingQuery();
  const [availableVotes, setAvailableVotes] = useState(Vote.allVoteTypes);
  const [vote, result] = useVoteMutation();

  useEffect(() => setAvailableVotes(getInitialAvailableVotes(data)), [data]);

  if (isLoading)
    return (
    <div className="flex flex-col space-y-3">
      <Skeleton className="h-[125px] w-[250px] rounded-xl" />
      <div className="space-y-2">
        <Skeleton className="h-4 w-[250px]" />
        <Skeleton className="h-4 w-[200px]" />
      </div>
    </div>
    )

  if (error)
  {
    return (<div>Coś się zesrao. Odśwież stronę.</div>);
  }

  const callVoteRoute = async (votes: number, movie: ConcreteMovie) => {
    const dto = { movieTitle: movie.movieName, votes, movieId: movie.movieId };
    const r = await vote(dto);
    if (r.error !== undefined) {
      toast.error("Coś poszło nie tak. Spróbuj ponownie.", {
        classNames: {
          description: "!text-foreground/80",
        },
        className: "text-5xl",
        richColors: true,
      });
      return;
    }
  }

  const voteCallback = (vote: Vote.Vote, movie: ConcreteMovie) => {
    if (!availableVotes.includes(vote)) {
      const newAvailableVotes = availableVotes.filter(x => x !== vote);
      callVoteRoute(0, movie);
      setAvailableVotes(newAvailableVotes);
    }
    else {
      const votesNumber = Vote.toNumber(vote);
      callVoteRoute(votesNumber, movie);
      setAvailableVotes([...availableVotes, vote]);
    }
  };

  const getCardProps = (movie: Movie) => {
    const movieVotes = (movie as ConcreteMovie)?.votes ?? 0;
    const [votesActive, votesAvailable] = movieVotes === 0 
    ? [[], availableVotes] 
    : [[Vote.fromNumber(movieVotes)], [Vote.fromNumber(movieVotes)]];
    const onVoteCallback = (vote: Vote.Vote) => voteCallback(vote, movie as ConcreteMovie);

    return {...props, movie, votesAvailable, votesActive, onVoteCallback};
  }

  let counter = 0;
  return (
    <>
    <Confetti isEnabled={availableVotes.length === 0}></Confetti>
    <div className="mt-10 ml-auto mr-25">
    {/* <Button onClick={onCarouselClick}>Set to carousel!</Button>  */}
    </div>
    <div className="flex flex-row flex-wrap justify-center mt-10">
      { data!.map(d => <MovieCard {...getCardProps(d)} key={counter++}></MovieCard>)}
    </div>
    </>
  );

  // function onCarouselClick() {
  //   displayMode === 'Carousel' ? setDisplayMode('Cards') : setDisplayMode('Carousel');
  // }
}

function getInitialAvailableVotes(movies?: Movie[]) {
  if (movies === undefined) {
    return Vote.allVoteTypes;
  }

  const usedVotes = movies
    .map(x => (x as ConcreteMovie)?.votes ?? 0)
    .filter(x => x !== 0)
    .map(x => Vote.fromNumber(x));
  const result = Vote.allVoteTypes.filter(x => !usedVotes.includes(x));
  return result;
}


const moviesList: React.FC<AppComponentProps> = (props) => { return <Layout><MoviesList {...props}/></Layout>}

export default moviesList;