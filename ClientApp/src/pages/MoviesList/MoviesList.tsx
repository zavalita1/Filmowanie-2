import { Layout, AppComponentProps } from "../Layout";
//import { useAppSelector } from "../../hooks/redux";
import { useGetCurrentVotingQuery } from '../../store/apis/Voting/votingApi';
import { useNavigate } from "react-router";
import { useEffect } from "react";
import { VotingStatus } from "../../consts/votingStatus";
import { Skeleton } from "../../components/ui/skeleton";
import { Card, CardHeader, CardTitle, CardDescription, CardAction, CardContent, CardFooter } from "../../components/ui/card";
import { Movie, ConcreteMovie, PlaceholderMovie } from "../../models/Movie";

const MoviesList: React.FC<AppComponentProps> = (props) => {
  const navigate = useNavigate();
  useEffect(() => {
    if (!props.userData)
      navigate('/');

    if (props.votingStatus !== VotingStatus.Voting)
      navigate('/');

  }, [props.userData, props.votingStatus]);

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
    <div className="flex flex-row flex-wrap mt-10"> 
      { data!.map(d => renderMovieCard(d))}
    </div>
  );
}

function renderMovieCard(movie: Movie) {
  const concreteMovie = movie as PlaceholderMovie;
  if (!!concreteMovie?.decade) {
    let message = `${concreteMovie.title} ${concreteMovie.decade}`;
    message = message.slice(0, -1) + 'X';
    return (
      <Card className="w-full max-w-5xl min-w-5xl m-2 mr-3 justify-center bg-gray-100">
        <CardHeader>
          <CardDescription className="text-2xl text-neutral-950 text-center"><b>{concreteMovie.title}</b></CardDescription>
        </CardHeader>
      </Card>
    );
  }
  
  return (
     <Card className="w-full max-w-5xl min-w-5xl m-2 mr-3">
      <CardHeader>
        <CardTitle>Card Title</CardTitle>
        <CardDescription>Card Description</CardDescription>
        <CardAction>Card Action</CardAction>
      </CardHeader>
      <CardContent>
        <p>Card Content</p>
      </CardContent>
      <CardFooter>
        <p>Card Footer</p>
      </CardFooter>
    </Card>
  );
}

const moviesList: React.FC<AppComponentProps> = (props) => { return <Layout><MoviesList {...props}/></Layout>}

export default moviesList;