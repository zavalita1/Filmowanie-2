import { Layout, AppComponentProps } from "../Layout";
//import { useAppSelector } from "../../hooks/redux";
import { useGetCurrentVotingQuery } from '../../store/apis/Voting/votingApi';
import { useNavigate } from "react-router";
import { useEffect, useState } from "react";
import { VotingStatus } from "../../consts/votingStatus";
import { Skeleton } from "../../components/ui/skeleton";
import { Card, CardHeader, CardTitle, CardDescription, CardAction, CardContent, CardFooter } from "../../components/ui/card";
import { Movie, ConcreteMovie, PlaceholderMovie } from "../../models/Movie";
import { Button } from "../../components/ui/button";

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
    // TODO
  }

  return (
    <>
    <div className="mt-10 ml-auto mr-25">
    <Button onClick={() => displayMode === 'Carousel' ? setDisplayMode('Cards') : setDisplayMode('Carousel')}>Set to carousel!</Button> 
    </div>
    <div className="flex flex-row flex-wrap justify-center mt-10">
      { data!.map(d => renderMovieCard(d))}
    </div>
    </>
  );
}

function renderMovieCard(movie: Movie) {
  const placeholderMovie = movie as PlaceholderMovie;
  if (!!placeholderMovie?.decade) {
    let message = `${placeholderMovie.title} ${placeholderMovie.decade}`;
    message = message.slice(0, -1) + 'X';
    return (
      <Card className="w-md m-2 mr-10 justify-center bg-gray-100">
        <CardHeader>
          <CardDescription className="text-2xl text-neutral-950 text-center"><b>{placeholderMovie.title}</b></CardDescription>
        </CardHeader>
      </Card>
    );
  }
  
  const concreteMovie = movie as ConcreteMovie;

  return (
     <Card className="w-md m-2 mr-10">
      <CardHeader>
        <CardTitle><b className="text-2xl">{concreteMovie.movieName}</b></CardTitle>
        <CardDescription>{concreteMovie.genres.join(", ")}</CardDescription>
      </CardHeader>
      <CardContent>
         <img className="justify-self-center" src={concreteMovie.posterUrl}></img>
      </CardContent>
      <CardFooter>
        <p>Card Footer</p>
      </CardFooter>
    </Card>
  );
}

const moviesList: React.FC<AppComponentProps> = (props) => { return <Layout><MoviesList {...props}/></Layout>}

export default moviesList;