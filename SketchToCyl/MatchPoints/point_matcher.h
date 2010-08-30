#ifndef POINT_MATCHER_H
#define POINT_MATCHER_H

#include "matrix.h"

/** Given two sets I={1,..,n} and J = {1,...,m} and the cost of matching element of I to element of J
*   in the matrix w, finds a match of size matchesCount with minimum cost, defined as a sum of costs 
*   w[i][j] for each matches i and j. Returns a matrix of size n x m, allocated by this function. The user is
*   responsible for freeing the allocated memory.
*/
Matrix<int> find_matches(const Matrix<double>& w);

#endif