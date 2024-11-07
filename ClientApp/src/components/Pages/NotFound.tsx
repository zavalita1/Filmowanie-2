import * as React from 'react';
import { useRouteError } from 'react-router-dom';

export default function NotFoundPage() {
  const error = useRouteError() as {statusText?: string, message?: string };
  console.error(error);

  return (
    <div id="error-page">
      <h1>O boże, o kurwa</h1>
      <p>Coś się popsuło</p>
      <p>
        <i>{error.statusText || error.message}</i>
      </p>
    </div>
  );
}